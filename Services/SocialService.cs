using KnewFate.Models;

namespace KnewFate.Services;

public interface ISocialService
{
    // Profile Management
    Task<UserProfile> GetUserProfileAsync(int userId);
    Task<UserProfile> CreateUserProfileAsync(int userId, string displayName, string country, string city = "");
    Task UpdateUserProfileAsync(UserProfile profile);
    Task<List<UserProfile>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20);
    Task<List<UserProfile>> GetNearbyUsersAsync(string country, string city, int skip = 0, int take = 20);
    Task<List<UserProfile>> GetRecommendedUsersAsync(int userId, int skip = 0, int take = 10);

    // Compatibility and Matching
    Task<CompatibilityMatch> CalculateCompatibilityAsync(int userId, int targetUserId);
    Task<List<CompatibilityMatch>> GetTopMatchesAsync(int userId, int limit = 10);
    Task<CompatibilityMatch> GetCompatibilityReportAsync(int userId, int targetUserId);
    Task SaveMatchAsync(CompatibilityMatch match);
    Task<bool> IsMutualMatchAsync(int userId, int targetUserId);

    // Social Interactions
    Task<bool> LikeUserAsync(int userId, int targetUserId);
    Task<bool> SuperLikeUserAsync(int userId, int targetUserId);
    Task<bool> ViewProfileAsync(int userId, int targetUserId);
    Task<bool> BlockUserAsync(int userId, int targetUserId);
    Task<bool> ReportUserAsync(int userId, int targetUserId, string reason);
    Task<List<UserInteraction>> GetUserInteractionsAsync(int userId, string interactionType = "");

    // Messaging
    Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content, string messageType = "Text");
    Task<List<ChatMessage>> GetChatHistoryAsync(int userId, int partnerId, int skip = 0, int take = 50);
    Task<List<ChatMessage>> GetRecentChatsAsync(int userId);
    Task MarkMessageAsReadAsync(int messageId);
    Task<bool> CanSendMessageAsync(int senderId, int receiverId);

    // Virtual Gifts
    Task<List<VirtualGift>> GetAvailableGiftsAsync();
    Task<bool> SendGiftAsync(int senderId, int receiverId, int giftId, int quantity, string message = "");
    Task<List<GiftTransaction>> GetReceivedGiftsAsync(int userId, int skip = 0, int take = 20);
    Task<List<GiftTransaction>> GetSentGiftsAsync(int userId, int skip = 0, int take = 20);

    // Premium Features
    Task<bool> IsUserPremiumAsync(int userId);
    Task<bool> CanAccessFeatureAsync(int userId, string featureName);
    Task<int> GetUserCreditsAsync(int userId);
    Task<bool> DeductCreditsAsync(int userId, int amount, string reason);
    Task AddCreditsAsync(int userId, int amount, string reason);

    // Social Feed
    Task<SocialFeed> CreatePostAsync(int userId, string postType, string content, string imageUrl = "");
    Task<List<SocialFeed>> GetFeedAsync(int userId, int skip = 0, int take = 20);
    Task<bool> LikePostAsync(int userId, int postId);
    Task<List<SocialFeed>> GetUserPostsAsync(int userId, int skip = 0, int take = 20);
}

public class SocialService : ISocialService
{
    private readonly IDatabaseService _databaseService;
    private readonly IZodiacService _zodiacService;
    private readonly IBaziService _baziService;
    private readonly IChartCalculationService _chartService;

    public SocialService(
        IDatabaseService databaseService,
        IZodiacService zodiacService,
        IBaziService baziService,
        IChartCalculationService chartService)
    {
        _databaseService = databaseService;
        _zodiacService = zodiacService;
        _baziService = baziService;
        _chartService = chartService;
    }

    // Profile Management
    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        return await db.Table<UserProfile>()
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync() ?? new UserProfile { UserId = userId };
    }

    public async Task<UserProfile> CreateUserProfileAsync(int userId, string displayName, string country, string city = "")
    {
        var profile = new UserProfile
        {
            UserId = userId,
            DisplayName = displayName,
            Country = country,
            City = city,
            IsPublic = true,
            IsOnline = true,
            LastSeen = DateTime.UtcNow,
            SubscriptionType = "Free",
            CreditsRemaining = 10 // Welcome credits
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertAsync(profile);
        
        return profile;
    }

    public async Task UpdateUserProfileAsync(UserProfile profile)
    {
        var db = await _databaseService.GetDatabaseAsync();
        await db.UpdateAsync(profile);
    }

    public async Task<List<UserProfile>> SearchUsersAsync(string searchTerm, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<UserProfile>()
            .Where(p => p.IsPublic && 
                       (p.DisplayName.Contains(searchTerm) || 
                        p.Country.Contains(searchTerm) || 
                        p.City.Contains(searchTerm)))
            .OrderByDescending(p => p.PopularityScore)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<UserProfile>> GetNearbyUsersAsync(string country, string city, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<UserProfile>()
            .Where(p => p.IsPublic && p.Country == country)
            .OrderBy(p => p.City == city ? 0 : 1) // Prioritize same city
            .ThenByDescending(p => p.PopularityScore)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<UserProfile>> GetRecommendedUsersAsync(int userId, int skip = 0, int take = 10)
    {
        var userProfile = await GetUserProfileAsync(userId);
        var db = await _databaseService.GetDatabaseAsync();
        
        // Get users from the same country who are public
        var candidates = await db.Table<UserProfile>()
            .Where(p => p.IsPublic && p.UserId != userId && p.Country == userProfile.Country)
            .ToListAsync();

        // Calculate compatibility scores and sort
        var recommendations = new List<(UserProfile profile, double score)>();
        
        foreach (var candidate in candidates)
        {
            var compatibility = await CalculateQuickCompatibilityAsync(userId, candidate.UserId);
            recommendations.Add((candidate, compatibility));
        }

        return recommendations
            .OrderByDescending(r => r.score)
            .Skip(skip)
            .Take(take)
            .Select(r => r.profile)
            .ToList();
    }

    // Compatibility and Matching
    public async Task<CompatibilityMatch> CalculateCompatibilityAsync(int userId, int targetUserId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        // Check if compatibility already calculated
        var existing = await db.Table<CompatibilityMatch>()
            .Where(m => (m.UserId == userId && m.TargetUserId == targetUserId) ||
                       (m.UserId == targetUserId && m.TargetUserId == userId))
            .FirstOrDefaultAsync();

        if (existing != null)
            return existing;

        // Calculate new compatibility
        var user = await _databaseService.GetUserByIdAsync(userId);
        var targetUser = await _databaseService.GetUserByIdAsync(targetUserId);
        
        if (user == null || targetUser == null)
            throw new ArgumentException("Invalid user IDs");

        var zodiacScore = await CalculateZodiacCompatibilityScore(user, targetUser);
        var baziScore = await CalculateBaziCompatibilityScore(user, targetUser);
        var numerologyScore = await CalculateNumerologyCompatibilityScore(user, targetUser);
        var personalityScore = await CalculatePersonalityCompatibilityScore(userId, targetUserId);

        var overallScore = (zodiacScore * 0.3 + baziScore * 0.3 + numerologyScore * 0.2 + personalityScore * 0.2);

        var compatibility = new CompatibilityMatch
        {
            UserId = userId,
            TargetUserId = targetUserId,
            OverallScore = overallScore,
            ZodiacScore = zodiacScore,
            BaziScore = baziScore,
            NumerologyScore = numerologyScore,
            PersonalityScore = personalityScore,
            MatchType = "Auto",
            Summary = GenerateCompatibilitySummary(overallScore, zodiacScore, baziScore),
            Advice = GenerateCompatibilityAdvice(overallScore),
            Challenges = GenerateCompatibilityChallenges(zodiacScore, baziScore),
            Strengths = GenerateCompatibilityStrengths(zodiacScore, baziScore),
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(compatibility);
        return compatibility;
    }

    public async Task<List<CompatibilityMatch>> GetTopMatchesAsync(int userId, int limit = 10)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<CompatibilityMatch>()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.OverallScore)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<CompatibilityMatch> GetCompatibilityReportAsync(int userId, int targetUserId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<CompatibilityMatch>()
            .Where(m => (m.UserId == userId && m.TargetUserId == targetUserId) ||
                       (m.UserId == targetUserId && m.TargetUserId == userId))
            .FirstOrDefaultAsync() ?? await CalculateCompatibilityAsync(userId, targetUserId);
    }

    public async Task SaveMatchAsync(CompatibilityMatch match)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        if (match.Id == 0)
            await db.InsertAsync(match);
        else
            await db.UpdateAsync(match);
    }

    public async Task<bool> IsMutualMatchAsync(int userId, int targetUserId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        var userLikesTarget = await db.Table<UserInteraction>()
            .Where(i => i.UserId == userId && i.TargetUserId == targetUserId && 
                       (i.InteractionType == "Like" || i.InteractionType == "SuperLike"))
            .CountAsync() > 0;

        var targetLikesUser = await db.Table<UserInteraction>()
            .Where(i => i.UserId == targetUserId && i.TargetUserId == userId && 
                       (i.InteractionType == "Like" || i.InteractionType == "SuperLike"))
            .CountAsync() > 0;

        return userLikesTarget && targetLikesUser;
    }

    // Social Interactions
    public async Task<bool> LikeUserAsync(int userId, int targetUserId)
    {
        return await RecordInteractionAsync(userId, targetUserId, "Like");
    }

    public async Task<bool> SuperLikeUserAsync(int userId, int targetUserId)
    {
        // Check if user has premium or credits
        var canSuperLike = await CanAccessFeatureAsync(userId, "SuperLike");
        if (!canSuperLike)
            return false;

        await DeductCreditsAsync(userId, 1, "SuperLike");
        return await RecordInteractionAsync(userId, targetUserId, "SuperLike");
    }

    public async Task<bool> ViewProfileAsync(int userId, int targetUserId)
    {
        return await RecordInteractionAsync(userId, targetUserId, "View");
    }

    public async Task<bool> BlockUserAsync(int userId, int targetUserId)
    {
        return await RecordInteractionAsync(userId, targetUserId, "Block");
    }

    public async Task<bool> ReportUserAsync(int userId, int targetUserId, string reason)
    {
        var interaction = new UserInteraction
        {
            UserId = userId,
            TargetUserId = targetUserId,
            InteractionType = "Report",
            CreatedAt = DateTime.UtcNow,
            MetaData = reason
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertAsync(interaction);
        return true;
    }

    public async Task<List<UserInteraction>> GetUserInteractionsAsync(int userId, string interactionType = "")
    {
        var db = await _databaseService.GetDatabaseAsync();
        var query = db.Table<UserInteraction>().Where(i => i.UserId == userId);
        
        if (!string.IsNullOrEmpty(interactionType))
            query = query.Where(i => i.InteractionType == interactionType);

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    // Messaging
    public async Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content, string messageType = "Text")
    {
        var canSend = await CanSendMessageAsync(senderId, receiverId);
        if (!canSend)
            throw new UnauthorizedAccessException("Cannot send message to this user");

        var message = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            MessageType = messageType,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertAsync(message);
        
        return message;
    }

    public async Task<List<ChatMessage>> GetChatHistoryAsync(int userId, int partnerId, int skip = 0, int take = 50)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<ChatMessage>()
            .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                       (m.SenderId == partnerId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetRecentChatsAsync(int userId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        // Get last message from each conversation
        return await db.Table<ChatMessage>()
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task MarkMessageAsReadAsync(int messageId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        var message = await db.GetAsync<ChatMessage>(messageId);
        
        if (message != null)
        {
            message.IsRead = true;
            await db.UpdateAsync(message);
        }
    }

    public async Task<bool> CanSendMessageAsync(int senderId, int receiverId)
    {
        // Check if mutual match or premium user
        var isMutual = await IsMutualMatchAsync(senderId, receiverId);
        var senderIsPremium = await IsUserPremiumAsync(senderId);
        
        return isMutual || senderIsPremium;
    }

    // Virtual Gifts
    public async Task<List<VirtualGift>> GetAvailableGiftsAsync()
    {
        return new List<VirtualGift>
        {
            new() { Id = 1, Name = "Rose", ImageUrl = "rose.png", CreditCost = 1, Category = "Flowers" },
            new() { Id = 2, Name = "Crystal Heart", ImageUrl = "crystal_heart.png", CreditCost = 3, Category = "Crystals" },
            new() { Id = 3, Name = "Lucky Charm", ImageUrl = "lucky_charm.png", CreditCost = 5, Category = "Charms" },
            new() { Id = 4, Name = "Golden Dragon", ImageUrl = "golden_dragon.png", CreditCost = 10, Category = "Premium" },
            new() { Id = 5, Name = "Destiny Star", ImageUrl = "destiny_star.png", CreditCost = 20, Category = "Premium" }
        };
    }

    public async Task<bool> SendGiftAsync(int senderId, int receiverId, int giftId, int quantity, string message = "")
    {
        var gifts = await GetAvailableGiftsAsync();
        var gift = gifts.FirstOrDefault(g => g.Id == giftId);
        
        if (gift == null)
            return false;

        var totalCost = gift.CreditCost * quantity;
        var canAfford = await DeductCreditsAsync(senderId, totalCost, $"Gift: {gift.Name}");
        
        if (!canAfford)
            return false;

        var transaction = new GiftTransaction
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            GiftId = giftId,
            Quantity = quantity,
            TotalCost = totalCost,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertAsync(transaction);
        
        return true;
    }

    public async Task<List<GiftTransaction>> GetReceivedGiftsAsync(int userId, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<GiftTransaction>()
            .Where(g => g.ReceiverId == userId)
            .OrderByDescending(g => g.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<GiftTransaction>> GetSentGiftsAsync(int userId, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<GiftTransaction>()
            .Where(g => g.SenderId == userId)
            .OrderByDescending(g => g.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    // Premium Features
    public async Task<bool> IsUserPremiumAsync(int userId)
    {
        var profile = await GetUserProfileAsync(userId);
        return profile.IsPremium && 
               (profile.PremiumExpiry == null || profile.PremiumExpiry > DateTime.UtcNow);
    }

    public async Task<bool> CanAccessFeatureAsync(int userId, string featureName)
    {
        var isPremium = await IsUserPremiumAsync(userId);
        var credits = await GetUserCreditsAsync(userId);

        return featureName switch
        {
            "SuperLike" => isPremium || credits >= 1,
            "UnlimitedLikes" => isPremium,
            "SeeWhoLiked" => isPremium,
            "AdvancedFilters" => isPremium,
            "PriorityMatching" => isPremium,
            "DetailedCompatibility" => isPremium || credits >= 2,
            _ => true // Basic features are free
        };
    }

    public async Task<int> GetUserCreditsAsync(int userId)
    {
        var profile = await GetUserProfileAsync(userId);
        return profile.CreditsRemaining;
    }

    public async Task<bool> DeductCreditsAsync(int userId, int amount, string reason)
    {
        var profile = await GetUserProfileAsync(userId);
        
        if (profile.CreditsRemaining < amount)
            return false;

        profile.CreditsRemaining -= amount;
        await UpdateUserProfileAsync(profile);
        
        return true;
    }

    public async Task AddCreditsAsync(int userId, int amount, string reason)
    {
        var profile = await GetUserProfileAsync(userId);
        profile.CreditsRemaining += amount;
        await UpdateUserProfileAsync(profile);
    }

    // Social Feed
    public async Task<SocialFeed> CreatePostAsync(int userId, string postType, string content, string imageUrl = "")
    {
        var post = new SocialFeed
        {
            UserId = userId,
            PostType = postType,
            Content = content,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertAsync(post);
        
        return post;
    }

    public async Task<List<SocialFeed>> GetFeedAsync(int userId, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<SocialFeed>()
            .Where(p => p.IsPublic)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<bool> LikePostAsync(int userId, int postId)
    {
        // This would typically update a likes table
        var db = await _databaseService.GetDatabaseAsync();
        var post = await db.GetAsync<SocialFeed>(postId);
        
        if (post != null)
        {
            post.Likes += 1;
            await db.UpdateAsync(post);
            return true;
        }
        
        return false;
    }

    public async Task<List<SocialFeed>> GetUserPostsAsync(int userId, int skip = 0, int take = 20)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        return await db.Table<SocialFeed>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    // Private Helper Methods
    private async Task<bool> RecordInteractionAsync(int userId, int targetUserId, string interactionType)
    {
        var db = await _databaseService.GetDatabaseAsync();
        
        // Check if interaction already exists
        var existing = await db.Table<UserInteraction>()
            .Where(i => i.UserId == userId && i.TargetUserId == targetUserId && i.InteractionType == interactionType)
            .FirstOrDefaultAsync();

        if (existing != null)
            return false; // Already exists

        var interaction = new UserInteraction
        {
            UserId = userId,
            TargetUserId = targetUserId,
            InteractionType = interactionType,
            CreatedAt = DateTime.UtcNow
        };

        await db.InsertAsync(interaction);
        return true;
    }

    private async Task<double> CalculateQuickCompatibilityAsync(int userId, int targetUserId)
    {
        // Quick compatibility calculation for recommendations
        try
        {
            var user = await _databaseService.GetUserByIdAsync(userId);
            var targetUser = await _databaseService.GetUserByIdAsync(targetUserId);
            
            if (user == null || targetUser == null || user.BirthDate == default || targetUser.BirthDate == default)
                return 0.5; // Default score

            var userSign = await _zodiacService.GetZodiacSignFromDateAsync(user.BirthDate);
            var targetSign = await _zodiacService.GetZodiacSignFromDateAsync(targetUser.BirthDate);
            
            return await _zodiacService.CalculateZodiacCompatibilityAsync(userSign, targetSign);
        }
        catch
        {
            return 0.5; // Default on error
        }
    }

    private async Task<double> CalculateZodiacCompatibilityScore(User user, User targetUser)
    {
        if (user.BirthDate == default || targetUser.BirthDate == default)
            return 0.5;

        var userSign = await _zodiacService.GetZodiacSignFromDateAsync(user.BirthDate);
        var targetSign = await _zodiacService.GetZodiacSignFromDateAsync(targetUser.BirthDate);
        
        return await _zodiacService.CalculateZodiacCompatibilityAsync(userSign, targetSign);
    }

    private async Task<double> CalculateBaziCompatibilityScore(User user, User targetUser)
    {
        try
        {
            // This would use the BaZi service to calculate compatibility
            // For now, return a placeholder
            return 0.7;
        }
        catch
        {
            return 0.5;
        }
    }

    private async Task<double> CalculateNumerologyCompatibilityScore(User user, User targetUser)
    {
        try
        {
            // Calculate life path numbers and compatibility
            if (user.BirthDate == default || targetUser.BirthDate == default)
                return 0.5;

            var userLifePath = CalculateLifePathNumber(user.BirthDate);
            var targetLifePath = CalculateLifePathNumber(targetUser.BirthDate);
            
            return CalculateNumerologyCompatibility(userLifePath, targetLifePath);
        }
        catch
        {
            return 0.5;
        }
    }

    private async Task<double> CalculatePersonalityCompatibilityScore(int userId, int targetUserId)
    {
        // This would compare personality profiles
        // For now, return a placeholder
        return 0.6;
    }

    private int CalculateLifePathNumber(DateTime birthDate)
    {
        var sum = birthDate.Year + birthDate.Month + birthDate.Day;
        while (sum > 9)
        {
            sum = sum.ToString().Sum(c => int.Parse(c.ToString()));
        }
        return sum;
    }

    private double CalculateNumerologyCompatibility(int lifePath1, int lifePath2)
    {
        var compatibilityMatrix = new Dictionary<(int, int), double>
        {
            [(1, 1)] = 0.6, [(1, 2)] = 0.8, [(1, 3)] = 0.9, [(1, 4)] = 0.5, [(1, 5)] = 0.7,
            [(1, 6)] = 0.6, [(1, 7)] = 0.4, [(1, 8)] = 0.8, [(1, 9)] = 0.7,
            [(2, 2)] = 0.7, [(2, 3)] = 0.5, [(2, 4)] = 0.9, [(2, 5)] = 0.4, [(2, 6)] = 0.8,
            [(2, 7)] = 0.6, [(2, 8)] = 0.7, [(2, 9)] = 0.8,
            [(3, 3)] = 0.6, [(3, 4)] = 0.4, [(3, 5)] = 0.9, [(3, 6)] = 0.7, [(3, 7)] = 0.5,
            [(3, 8)] = 0.6, [(3, 9)] = 0.8,
            // Add more combinations...
        };

        var key = (Math.Min(lifePath1, lifePath2), Math.Max(lifePath1, lifePath2));
        return compatibilityMatrix.ContainsKey(key) ? compatibilityMatrix[key] : 0.5;
    }

    private string GenerateCompatibilitySummary(double overallScore, double zodiacScore, double baziScore)
    {
        return overallScore switch
        {
            >= 0.9 => "完美匹配！你们在各个方面都非常合拍，是天造地设的一对。",
            >= 0.8 => "极佳匹配！你们的性格和价值观高度契合，有很大的发展潜力。",
            >= 0.7 => "良好匹配！虽然有一些差异，但这些差异能够互补，关系会很稳定。",
            >= 0.6 => "一般匹配。需要更多的沟通和理解，但通过努力可以建立良好关系。",
            >= 0.5 => "具有挑战性的匹配。需要双方付出更多努力来克服差异。",
            _ => "匹配度较低。可能在很多方面存在分歧，需要慎重考虑。"
        };
    }

    private string GenerateCompatibilityAdvice(double overallScore)
    {
        return overallScore switch
        {
            >= 0.8 => "珍惜这份难得的缘分，保持开放和诚实的沟通。",
            >= 0.6 => "多花时间了解对方，培养共同兴趣，增进感情。",
            _ => "需要更多的耐心和理解，专注于寻找共同点。"
        };
    }

    private string GenerateCompatibilityChallenges(double zodiacScore, double baziScore)
    {
        var challenges = new List<string>();
        
        if (zodiacScore < 0.6)
            challenges.Add("性格差异较大，需要更多包容");
        if (baziScore < 0.6)
            challenges.Add("人生节奏可能不同，需要协调");
        
        return challenges.Any() ? string.Join("；", challenges) : "暂未发现明显挑战";
    }

    private string GenerateCompatibilityStrengths(double zodiacScore, double baziScore)
    {
        var strengths = new List<string>();
        
        if (zodiacScore > 0.7)
            strengths.Add("性格互补，能够相互理解");
        if (baziScore > 0.7)
            strengths.Add("人生理念相近，目标一致");
        
        return strengths.Any() ? string.Join("；", strengths) : "需要进一步发掘彼此的优势";
    }
}
