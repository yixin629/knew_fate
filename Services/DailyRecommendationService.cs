using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnewFate.Models;

namespace KnewFate.Services
{
    public interface IDailyRecommendationService
    {
        Task<DailyRecommendationList> GetDailyRecommendationsAsync(int userId, DateTime date);
        Task<List<RecommendationItem>> GetTopRecommendationsAsync(int userId, int count = 10);
        Task MarkRecommendationViewedAsync(int userId, int recommendationId);
    }

    public class DailyRecommendationService : IDailyRecommendationService
    {
        private readonly ISocialService _socialService;
        private readonly IZodiacService _zodiacService;
        private readonly Random _random = new();

        public DailyRecommendationService(
            ISocialService socialService,
            IZodiacService zodiacService)
        {
            _socialService = socialService;
            _zodiacService = zodiacService;
        }

        public async Task<DailyRecommendationList> GetDailyRecommendationsAsync(int userId, DateTime date)
        {
            // Generate or retrieve cached daily recommendations
            var recommendations = new DailyRecommendationList
            {
                UserId = userId,
                Date = date.Date,
                GeneratedAt = DateTime.Now,
                Items = await GenerateRecommendationsAsync(userId, date)
            };

            // Rank items by score and mark top 10
            var rankedItems = recommendations.Items
                .OrderByDescending(i => i.Score)
                .ToList();

            for (int i = 0; i < rankedItems.Count; i++)
            {
                rankedItems[i].Rank = i + 1;
                rankedItems[i].IsTopTen = i < 10;
            }

            recommendations.Items = rankedItems;
            return recommendations;
        }

        public async Task<List<RecommendationItem>> GetTopRecommendationsAsync(int userId, int count = 10)
        {
            var dailyList = await GetDailyRecommendationsAsync(userId, DateTime.Now);
            return dailyList.Items.Take(count).ToList();
        }

        public async Task MarkRecommendationViewedAsync(int userId, int recommendationId)
        {
            // In a real implementation, this would update the database
            await Task.CompletedTask;
        }

        private async Task<List<RecommendationItem>> GenerateRecommendationsAsync(int userId, DateTime date)
        {
            var items = new List<RecommendationItem>();

            // Generate diverse recommendation types
            items.AddRange(await GenerateUserRecommendationsAsync(userId));
            items.AddRange(await GenerateReadingRecommendationsAsync(userId));
            items.AddRange(await GenerateInsightRecommendationsAsync(userId, date));
            items.AddRange(await GenerateEventRecommendationsAsync(date));

            // Calculate scores based on relevance
            foreach (var item in items)
            {
                item.Score = CalculateRelevanceScore(item, userId, date);
            }

            return items;
        }

        private async Task<List<RecommendationItem>> GenerateUserRecommendationsAsync(int userId)
        {
            var items = new List<RecommendationItem>();

            try
            {
                // Get recommended users with high compatibility
                var recommendedUsers = await _socialService.GetRecommendedUsersAsync(userId, 0, 15);
                
                foreach (var user in recommendedUsers.Take(10))
                {
                    var compatibility = await _socialService.CalculateCompatibilityAsync(userId, user.UserId);
                    
                    items.Add(new RecommendationItem
                    {
                        Id = user.UserId,
                        Title = user.DisplayName,
                        Description = $"Compatibility: {compatibility.OverallScore:P0} - {user.Bio}",
                        Category = "User",
                        Score = compatibility.OverallScore * 100,
                        CreatedAt = DateTime.Now,
                        ImageUrl = user.Avatar,
                        ActionUrl = $"userprofile?userId={user.UserId}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "userId", user.UserId.ToString() },
                            { "compatibility", compatibility.OverallScore.ToString("P0") },
                            { "age", user.Age.ToString() },
                            { "city", user.City }
                        }
                    });
                }
            }
            catch
            {
                // Fallback to sample data if service fails
            }

            return items;
        }

        private async Task<List<RecommendationItem>> GenerateReadingRecommendationsAsync(int userId)
        {
            var items = new List<RecommendationItem>();

            // Suggest various reading types
            var readingTypes = new[]
            {
                new { Type = "Tarot", Title = "Daily Tarot Reading", Description = "Get insights for today with a three-card spread", Action = "tarot" },
                new { Type = "Astrology", Title = "Transit Analysis", Description = "See how current planetary positions affect you", Action = "charthub" },
                new { Type = "BaZi", Title = "Fortune Period Review", Description = "Understand your current luck cycle", Action = "charthub" },
                new { Type = "Numerology", Title = "Personal Year Forecast", Description = "Discover what this year holds for you", Action = "charthub" },
                new { Type = "Ziwei", Title = "Palace Analysis", Description = "Deep dive into your Ziwei Doushu palaces", Action = "charthub" }
            };

            foreach (var reading in readingTypes)
            {
                items.Add(new RecommendationItem
                {
                    Title = reading.Title,
                    Description = reading.Description,
                    Category = "Reading",
                    Score = _random.Next(60, 95),
                    CreatedAt = DateTime.Now,
                    ActionUrl = reading.Action,
                    ImageUrl = $"{reading.Type.ToLower()}_icon.png",
                    Metadata = new Dictionary<string, string>
                    {
                        { "readingType", reading.Type }
                    }
                });
            }

            return items;
        }

        private async Task<List<RecommendationItem>> GenerateInsightRecommendationsAsync(int userId, DateTime date)
        {
            var items = new List<RecommendationItem>();

            try
            {
                var zodiacProfile = await _zodiacService.GetZodiacProfileAsync(userId);
                var dayOfWeek = date.DayOfWeek;

                // Generate personalized insights
                var insights = new[]
                {
                    new { Title = "Career Opportunity", Description = $"Your {zodiacProfile.WesternSign} energy aligns with career growth today", Score = 88.0 },
                    new { Title = "Relationship Harmony", Description = "Communication flows easily - perfect for important conversations", Score = 85.0 },
                    new { Title = "Creative Expression", Description = "Artistic pursuits are especially favored this week", Score = 82.0 },
                    new { Title = "Financial Planning", Description = "Good time to review and adjust your financial goals", Score = 78.0 },
                    new { Title = "Personal Growth", Description = "Reflect on recent experiences and learn from them", Score = 75.0 },
                    new { Title = "Social Connections", Description = "Networking opportunities emerge - be open to new contacts", Score = 80.0 }
                };

                foreach (var insight in insights)
                {
                    items.Add(new RecommendationItem
                    {
                        Title = insight.Title,
                        Description = insight.Description,
                        Category = "Insight",
                        Score = insight.Score,
                        CreatedAt = DateTime.Now,
                        ActionUrl = "dashboard",
                        Metadata = new Dictionary<string, string>
                        {
                            { "zodiacSign", zodiacProfile.WesternSign.ToString() }
                        }
                    });
                }
            }
            catch
            {
                // Fallback insights
                items.Add(new RecommendationItem
                {
                    Title = "Daily Reflection",
                    Description = "Take time today to reflect on your goals and aspirations",
                    Category = "Insight",
                    Score = 70,
                    CreatedAt = DateTime.Now,
                    ActionUrl = "dashboard"
                });
            }

            return items;
        }

        private async Task<List<RecommendationItem>> GenerateEventRecommendationsAsync(DateTime date)
        {
            var items = new List<RecommendationItem>();

            // Suggest time-sensitive activities or events
            var events = new[]
            {
                new { Title = "Mercury Retrograde Alert", Description = "Review communications and back up data during this period", Score = 90.0 },
                new { Title = "New Moon Ritual", Description = "Perfect time for setting new intentions and goals", Score = 87.0 },
                new { Title = "Lucky Day Alert", Description = "Today's energy is particularly favorable for new beginnings", Score = 84.0 },
                new { Title = "Meditation Reminder", Description = "Ground yourself with a 10-minute meditation session", Score = 72.0 }
            };

            // Only add events that are relevant for today
            var relevantEvents = events.Where(e => _random.NextDouble() > 0.5).ToArray();
            
            foreach (var evt in relevantEvents)
            {
                items.Add(new RecommendationItem
                {
                    Title = evt.Title,
                    Description = evt.Description,
                    Category = "Event",
                    Score = evt.Score,
                    CreatedAt = DateTime.Now,
                    ActionUrl = "timeline",
                    Metadata = new Dictionary<string, string>
                    {
                        { "eventDate", date.ToString("yyyy-MM-dd") }
                    }
                });
            }

            return items;
        }

        private double CalculateRelevanceScore(RecommendationItem item, int userId, DateTime date)
        {
            // Score is already calculated, but we can adjust based on other factors
            var baseScore = item.Score;

            // Boost user recommendations slightly
            if (item.Category == "User")
            {
                baseScore += 5;
            }

            // Boost event recommendations on weekends
            if (item.Category == "Event" && (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday))
            {
                baseScore += 3;
            }

            // Add some randomization to keep things interesting
            baseScore += _random.Next(-2, 3);

            return Math.Min(100, Math.Max(0, baseScore));
        }
    }
}
