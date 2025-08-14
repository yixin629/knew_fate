using SQLite;
using System.Linq;

namespace KnewFate.Services;

// User Preferences Service
public interface IUserPreferencesService
{
    Task<string> GetStringAsync(string key, string defaultValue = "");
    Task SetStringAsync(string key, string value);
    Task<bool> GetBoolAsync(string key, bool defaultValue = false);
    Task SetBoolAsync(string key, bool value);
    Task<int> GetIntAsync(string key, int defaultValue = 0);
    Task SetIntAsync(string key, int value);
    Task<double> GetDoubleAsync(string key, double defaultValue = 0.0);
    Task SetDoubleAsync(string key, double value);
    Task RemoveAsync(string key);
    Task ClearAllAsync();
}

public class UserPreferencesService : IUserPreferencesService
{
    public async Task<string> GetStringAsync(string key, string defaultValue = "")
    {
        return await SecureStorage.GetAsync(key) ?? defaultValue;
    }

    public async Task SetStringAsync(string key, string value)
    {
        await SecureStorage.SetAsync(key, value);
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var value = await SecureStorage.GetAsync(key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task SetBoolAsync(string key, bool value)
    {
        await SecureStorage.SetAsync(key, value.ToString());
    }

    public async Task<int> GetIntAsync(string key, int defaultValue = 0)
    {
        var value = await SecureStorage.GetAsync(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task SetIntAsync(string key, int value)
    {
        await SecureStorage.SetAsync(key, value.ToString());
    }

    public async Task<double> GetDoubleAsync(string key, double defaultValue = 0.0)
    {
        var value = await SecureStorage.GetAsync(key);
        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task SetDoubleAsync(string key, double value)
    {
        await SecureStorage.SetAsync(key, value.ToString());
    }

    public async Task RemoveAsync(string key)
    {
        SecureStorage.Remove(key);
        await Task.CompletedTask;
    }

    public async Task ClearAllAsync()
    {
        SecureStorage.RemoveAll();
        await Task.CompletedTask;
    }
}

// Database Service
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<T> GetAsync<T>(int id) where T : class, new();
    Task<List<T>> GetAllAsync<T>() where T : class, new();
    Task<int> SaveAsync<T>(T item) where T : class, new();
    Task<int> DeleteAsync<T>(T item) where T : class, new();
    Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : class, new();
    
    // User management methods
    Task<Models.User> GetUserByEmailAsync(string email);
    Task<Models.User> GetUserByIdAsync(int userId);
    Task<int> AddUserAsync(Models.User user);
    Task<int> UpdateUserAsync(Models.User user);
    
    // Password reset methods
    Task<int> AddPasswordResetRequestAsync(Models.PasswordResetRequest request);
    Task<Models.PasswordResetRequest> GetPasswordResetRequestAsync(string token);
    Task<int> UpdatePasswordResetRequestAsync(Models.PasswordResetRequest request);
    
    // SQLite connection for advanced queries
    Task<SQLiteAsyncConnection> GetDatabaseAsync();
}

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "knewfate.db3");
    }

    public async Task InitializeAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath);
        
        // Create tables
        await _database.CreateTableAsync<Models.User>();
        await _database.CreateTableAsync<Models.BirthData>();
        await _database.CreateTableAsync<Models.ChartBase>();
        await _database.CreateTableAsync<Models.PersonalityProfile>();
        await _database.CreateTableAsync<Models.FusionVector>();
        await _database.CreateTableAsync<Models.RelationshipReport>();
        await _database.CreateTableAsync<Models.TarotSpread>();
        await _database.CreateTableAsync<Models.TimelinePoint>();
        await _database.CreateTableAsync<Models.UserFeedback>();
        await _database.CreateTableAsync<Models.Purchase>();
        await _database.CreateTableAsync<Models.Subscription>();
        
        // Authentication tables
        await _database.CreateTableAsync<Models.PasswordResetRequest>();
        
        // Social and Zodiac tables
        await _database.CreateTableAsync<Models.ZodiacProfile>();
        await _database.CreateTableAsync<Models.UserProfile>();
        await _database.CreateTableAsync<Models.CompatibilityMatch>();
        await _database.CreateTableAsync<Models.ChatMessage>();
        await _database.CreateTableAsync<Models.UserInteraction>();
        await _database.CreateTableAsync<Models.SocialFeed>();
        await _database.CreateTableAsync<Models.VirtualGift>();
        await _database.CreateTableAsync<Models.GiftTransaction>();
    }

    public async Task<T> GetAsync<T>(int id) where T : class, new()
    {
        await InitializeAsync();
        return await _database.FindAsync<T>(id);
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class, new()
    {
        await InitializeAsync();
        return await _database.Table<T>().ToListAsync();
    }

    public async Task<int> SaveAsync<T>(T item) where T : class, new()
    {
        await InitializeAsync();
        
        // Check if item has Id property and if it's 0 (new item)
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            var id = (int)(idProperty.GetValue(item) ?? 0);
            if (id != 0)
            {
                return await _database.UpdateAsync(item);
            }
        }
        
        return await _database.InsertAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item) where T : class, new()
    {
        await InitializeAsync();
        return await _database.DeleteAsync(item);
    }

    public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : class, new()
    {
        await InitializeAsync();
        return await _database.QueryAsync<T>(query, args);
    }

    // User management methods
    public async Task<Models.User> GetUserByEmailAsync(string email)
    {
        await InitializeAsync();
        var users = await _database.QueryAsync<Models.User>("SELECT * FROM User WHERE Email = ?", email);
        return users.FirstOrDefault();
    }

    public async Task<Models.User> GetUserByIdAsync(int userId)
    {
        await InitializeAsync();
        return await _database.FindAsync<Models.User>(userId);
    }

    public async Task<int> AddUserAsync(Models.User user)
    {
        await InitializeAsync();
        return await _database.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(Models.User user)
    {
        await InitializeAsync();
        return await _database.UpdateAsync(user);
    }

    // Password reset methods
    public async Task<int> AddPasswordResetRequestAsync(Models.PasswordResetRequest request)
    {
        await InitializeAsync();
        return await _database.InsertAsync(request);
    }

    public async Task<Models.PasswordResetRequest> GetPasswordResetRequestAsync(string token)
    {
        await InitializeAsync();
        var requests = await _database.QueryAsync<Models.PasswordResetRequest>("SELECT * FROM PasswordResetRequest WHERE Token = ?", token);
        return requests.FirstOrDefault();
    }

    public async Task<int> UpdatePasswordResetRequestAsync(Models.PasswordResetRequest request)
    {
        await InitializeAsync();
        return await _database.UpdateAsync(request);
    }

    // SQLite connection for advanced queries
    public async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        await InitializeAsync();
        return _database;
    }
}

// Chart Calculation Service
public interface IChartCalculationService
{
    Task<Models.BaziChart> CalculateBaziChartAsync(Models.BirthData birthData);
    Task<Models.AstrologyChart> CalculateAstrologyChartAsync(Models.BirthData birthData);
    Task<Dictionary<string, object>> CalculateNumerologyAsync(Models.BirthData birthData, string fullName);
    Task<Models.PersonalityProfile> AnalyzePersonalityAsync(Models.BirthData birthData);
}

public class ChartCalculationService : IChartCalculationService
{
    public async Task<Models.BaziChart> CalculateBaziChartAsync(Models.BirthData birthData)
    {
        // TODO: Implement actual BaZi calculation
        // This is a simplified mock implementation
        await Task.Delay(100); // Simulate calculation time
        
        return new Models.BaziChart
        {
            Type = Models.ChartType.Bazi,
            YearPillar = "甲子",
            MonthPillar = "乙丑",
            DayPillar = "丙寅",
            HourPillar = "丁卯",
            DayMaster = "丙",
            TenGods = new List<string> { "正官", "偏印", "劫财", "食神" },
            FiveElements = new List<string> { "木", "火", "土", "金", "水" },
            LuckPeriods = new List<Models.LuckPeriod>
            {
                new() { StartAge = 5, EndAge = 14, Stem = "戊", Branch = "辰", Description = "早年求学期" },
                new() { StartAge = 15, EndAge = 24, Stem = "己", Branch = "巳", Description = "青年发展期" }
            }
        };
    }

    public async Task<Models.AstrologyChart> CalculateAstrologyChartAsync(Models.BirthData birthData)
    {
        // TODO: Implement actual astrology calculation using Swiss Ephemeris or similar
        await Task.Delay(100);
        
        return new Models.AstrologyChart
        {
            Type = Models.ChartType.Astrology,
            Planets = new List<Models.Planet>
            {
                new() { Name = "Sun", Degree = 15.5, Sign = "Leo", House = 1, IsRetrograde = false },
                new() { Name = "Moon", Degree = 22.3, Sign = "Cancer", House = 12, IsRetrograde = false },
                new() { Name = "Mercury", Degree = 8.7, Sign = "Virgo", House = 2, IsRetrograde = true }
            },
            Houses = new List<Models.House>
            {
                new() { Number = 1, Sign = "Leo", CuspDegree = 10.0 },
                new() { Number = 2, Sign = "Virgo", CuspDegree = 5.5 }
            },
            Aspects = new List<Models.Aspect>
            {
                new() { Planet1 = "Sun", Planet2 = "Moon", Type = "Trine", Orb = 2.5, IsApplying = true }
            },
            Ascendant = "Leo",
            Midheaven = "Taurus"
        };
    }

    public async Task<Dictionary<string, object>> CalculateNumerologyAsync(Models.BirthData birthData, string fullName)
    {
        await Task.Delay(50);
        
        // Simple numerology calculation
        var birthDate = birthData.SolarDate;
        var lifePath = CalculateLifePathNumber(birthDate);
        var expression = CalculateExpressionNumber(fullName);
        
        return new Dictionary<string, object>
        {
            {"LifePath", lifePath},
            {"Expression", expression},
            {"SoulUrge", CalculateSoulUrgeNumber(fullName)},
            {"Personality", CalculatePersonalityNumber(fullName)}
        };
    }

    public async Task<Models.PersonalityProfile> AnalyzePersonalityAsync(Models.BirthData birthData)
    {
        await Task.Delay(100);
        
        // Mock personality analysis based on birth data
        return new Models.PersonalityProfile
        {
            MbtiType = "INTJ", // This would be calculated based on chart analysis
            BigFive = new Models.BigFiveVector
            {
                Openness = 0.75,
                Conscientiousness = 0.65,
                Extraversion = 0.45,
                Agreeableness = 0.55,
                Neuroticism = 0.35
            },
            EnneagramType = 5,
            HumanDesignType = "Manifesting Generator",
            UpdatedAt = DateTime.UtcNow
        };
    }

    private int CalculateLifePathNumber(DateTime birthDate)
    {
        var sum = birthDate.Day + birthDate.Month + birthDate.Year;
        while (sum > 9 && sum != 11 && sum != 22 && sum != 33)
        {
            sum = sum.ToString().Sum(c => c - '0');
        }
        return sum;
    }

    private int CalculateExpressionNumber(string fullName)
    {
        var values = new Dictionary<char, int>
        {
            {'A', 1}, {'B', 2}, {'C', 3}, {'D', 4}, {'E', 5}, {'F', 6}, {'G', 7}, {'H', 8}, {'I', 9},
            {'J', 1}, {'K', 2}, {'L', 3}, {'M', 4}, {'N', 5}, {'O', 6}, {'P', 7}, {'Q', 8}, {'R', 9},
            {'S', 1}, {'T', 2}, {'U', 3}, {'V', 4}, {'W', 5}, {'X', 6}, {'Y', 7}, {'Z', 8}
        };

        var sum = fullName.ToUpper().Where(char.IsLetter).Sum(c => values.GetValueOrDefault(c, 0));
        while (sum > 9 && sum != 11 && sum != 22 && sum != 33)
        {
            sum = sum.ToString().Sum(c => c - '0');
        }
        return sum;
    }

    private int CalculateSoulUrgeNumber(string fullName)
    {
        var vowels = "AEIOU";
        var values = new Dictionary<char, int>
        {
            {'A', 1}, {'E', 5}, {'I', 9}, {'O', 6}, {'U', 3}
        };

        var sum = fullName.ToUpper().Where(c => vowels.Contains(c)).Sum(c => values.GetValueOrDefault(c, 0));
        while (sum > 9 && sum != 11 && sum != 22 && sum != 33)
        {
            sum = sum.ToString().Sum(c => c - '0');
        }
        return sum;
    }

    private int CalculatePersonalityNumber(string fullName)
    {
        var consonants = "BCDFGHJKLMNPQRSTVWXYZ";
        var values = new Dictionary<char, int>
        {
            {'B', 2}, {'C', 3}, {'D', 4}, {'F', 6}, {'G', 7}, {'H', 8}, {'J', 1}, {'K', 2},
            {'L', 3}, {'M', 4}, {'N', 5}, {'P', 7}, {'Q', 8}, {'R', 9}, {'S', 1}, {'T', 2},
            {'V', 4}, {'W', 5}, {'X', 6}, {'Y', 7}, {'Z', 8}
        };

        var sum = fullName.ToUpper().Where(c => consonants.Contains(c)).Sum(c => values.GetValueOrDefault(c, 0));
        while (sum > 9 && sum != 11 && sum != 22 && sum != 33)
        {
            sum = sum.ToString().Sum(c => c - '0');
        }
        return sum;
    }
}

// Fusion Engine Service
public interface IFusionEngineService
{
    Task<Models.FusionVector> GenerateFusionVectorAsync(int userId);
    Task<string> GenerateNarrativeAsync(Models.FusionVector fusionVector, string targetDimension);
    Task<double> CalculateConfidenceAsync(List<Models.ChartBase> charts, string dimension);
}

public class FusionEngineService : IFusionEngineService
{
    private readonly IDatabaseService _databaseService;
    private readonly IChartCalculationService _chartCalculationService;

    public FusionEngineService(IDatabaseService databaseService, IChartCalculationService chartCalculationService)
    {
        _databaseService = databaseService;
        _chartCalculationService = chartCalculationService;
    }

    public async Task<Models.FusionVector> GenerateFusionVectorAsync(int userId)
    {
        // Get all charts for user
        var charts = await _databaseService.QueryAsync<Models.ChartBase>("SELECT * FROM ChartBase WHERE UserId = ?", userId);
        var personalityProfile = await _databaseService.QueryAsync<Models.PersonalityProfile>("SELECT * FROM PersonalityProfile WHERE UserId = ? ORDER BY UpdatedAt DESC LIMIT 1", userId);

        // Mock fusion calculation
        await Task.Delay(200); // Simulate complex calculation

        return new Models.FusionVector
        {
            UserId = userId,
            Career = new Models.CareerDimension
            {
                CreativityDrive = 0.75,
                AnalyticalTendency = 0.65,
                LeadershipPotential = 0.55,
                RiskTolerance = 0.45,
                CollaborationStyle = 0.70,
                SuitableIndustries = new List<string> { "Technology", "Creative Arts", "Consulting" }
            },
            Relationship = new Models.RelationshipDimension
            {
                AttachmentStyle = 0.60,
                CommunicationStyle = 0.70,
                ConflictResolution = 0.55,
                EmotionalExpression = 0.65,
                IntimacyPreference = 0.75
            },
            Energy = new Models.EnergeticBalance
            {
                FiveElements = new Models.FiveElementBalance
                {
                    Wood = 0.20,
                    Fire = 0.25,
                    Earth = 0.15,
                    Metal = 0.20,
                    Water = 0.20
                },
                AstroElements = new Models.AstrologicalElementBalance
                {
                    Fire = 0.30,
                    Earth = 0.20,
                    Air = 0.25,
                    Water = 0.25
                },
                EnergyType = "Manifesting Generator"
            },
            Decision = new Models.DecisionStrategy
            {
                IntuitiveTendency = 0.70,
                AnalyticalTendency = 0.60,
                EmotionalWeight = 0.55,
                SocialInfluence = 0.45,
                PrimaryStrategy = "Intuitive-Analytical Hybrid"
            },
            ConfidenceScores = new List<double> { 0.75, 0.68, 0.72, 0.65 },
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async Task<string> GenerateNarrativeAsync(Models.FusionVector fusionVector, string targetDimension)
    {
        await Task.Delay(100);

        // This would integrate with LLM in a real implementation
        return targetDimension.ToLower() switch
        {
            "career" => GenerateCareerNarrative(fusionVector.Career),
            "relationship" => GenerateRelationshipNarrative(fusionVector.Relationship),
            "energy" => GenerateEnergyNarrative(fusionVector.Energy),
            "decision" => GenerateDecisionNarrative(fusionVector.Decision),
            _ => "Your unique combination of traits suggests a multifaceted approach to life's challenges."
        };
    }

    public async Task<double> CalculateConfidenceAsync(List<Models.ChartBase> charts, string dimension)
    {
        await Task.Delay(50);
        
        // Calculate confidence based on data completeness and system agreement
        var dataCompleteness = charts.Count / 5.0; // Assuming 5 main systems
        var systemAgreement = 0.75; // Mock system agreement score
        var timeAccuracy = 0.80; // Mock time accuracy factor
        
        return Math.Min(1.0, dataCompleteness * systemAgreement * timeAccuracy);
    }

    private string GenerateCareerNarrative(Models.CareerDimension career)
    {
        return $"Your professional profile shows strong creative drive ({career.CreativityDrive:P0}) balanced with analytical capabilities ({career.AnalyticalTendency:P0}). " +
               $"Leadership potential is {(career.LeadershipPotential > 0.6 ? "strong" : "developing")}, suggesting " +
               $"{(career.LeadershipPotential > 0.6 ? "management or entrepreneurial" : "collaborative specialist")} roles suit you well. " +
               $"Consider industries like {string.Join(", ", career.SuitableIndustries)} for optimal career fulfillment.";
    }

    private string GenerateRelationshipNarrative(Models.RelationshipDimension relationship)
    {
        return $"In relationships, you demonstrate {(relationship.CommunicationStyle > 0.6 ? "excellent" : "developing")} communication skills " +
               $"with a {(relationship.EmotionalExpression > 0.6 ? "open" : "measured")} approach to emotional expression. " +
               $"Your conflict resolution style tends to be {(relationship.ConflictResolution > 0.6 ? "collaborative and effective" : "thoughtful and careful")}. " +
               $"Intimacy preferences suggest you value {(relationship.IntimacyPreference > 0.6 ? "deep, meaningful connections" : "balanced personal space and closeness")}.";
    }

    private string GenerateEnergyNarrative(Models.EnergeticBalance energy)
    {
        var dominantElement = GetDominantFiveElement(energy.FiveElements);
        return $"Your energetic profile is dominated by {dominantElement} element, suggesting " +
               $"{GetElementCharacteristics(dominantElement)}. " +
               $"As a {energy.EnergyType}, you work best when {GetEnergyTypeAdvice(energy.EnergyType)}.";
    }

    private string GenerateDecisionNarrative(Models.DecisionStrategy decision)
    {
        return $"Your decision-making style is primarily {decision.PrimaryStrategy}, " +
               $"combining {decision.IntuitiveTendency:P0} intuitive processing with {decision.AnalyticalTendency:P0} analytical thinking. " +
               $"You {(decision.EmotionalWeight > 0.6 ? "highly value" : "moderately consider")} emotional factors and " +
               $"{(decision.SocialInfluence > 0.6 ? "are significantly influenced by" : "maintain independence from")} social input in your choices.";
    }

    private string GetDominantFiveElement(Models.FiveElementBalance elements)
    {
        var max = Math.Max(Math.Max(elements.Wood, elements.Fire), 
                          Math.Max(Math.Max(elements.Earth, elements.Metal), elements.Water));
        
        if (Math.Abs(max - elements.Wood) < 0.01) return "Wood";
        if (Math.Abs(max - elements.Fire) < 0.01) return "Fire";
        if (Math.Abs(max - elements.Earth) < 0.01) return "Earth";
        if (Math.Abs(max - elements.Metal) < 0.01) return "Metal";
        return "Water";
    }

    private string GetElementCharacteristics(string element)
    {
        return element switch
        {
            "Wood" => "growth-oriented, creative, and flexible nature",
            "Fire" => "passionate, expressive, and dynamic energy",
            "Earth" => "stable, practical, and nurturing qualities",
            "Metal" => "structured, precise, and focused approach",
            "Water" => "adaptive, intuitive, and flowing wisdom",
            _ => "balanced and harmonious energy"
        };
    }

    private string GetEnergyTypeAdvice(string energyType)
    {
        return energyType switch
        {
            "Manifesting Generator" => "responding to opportunities while maintaining multiple interests",
            "Generator" => "responding to what lights you up and following your satisfaction",
            "Manifestor" => "initiating and informing others of your actions",
            "Projector" => "waiting for invitations and focusing on guidance",
            "Reflector" => "taking time for lunar cycles and reflecting community health",
            _ => "following your authentic nature and inner wisdom"
        };
    }
}
