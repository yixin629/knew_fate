using System.ComponentModel.DataAnnotations;
using SQLite;

namespace KnewFate.Models;

// Core User and Birth Data Models
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Unique]
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PreferredLanguage { get; set; } = "en-US";
    public bool IsPremium { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public DateTime? LastActiveAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    
    // Authentication properties
    public DateTime BirthDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    [Ignore]
    public BirthData BirthData { get; set; } = new();
}

public class BirthData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime SolarDate { get; set; }
    public DateTime LunarDate { get; set; }
    public TimeSpan BirthTime { get; set; }
    public string BirthLocation { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public int TimeAccuracyLevel { get; set; } // 1-5, 5 being most accurate
    public DateTime UpdatedAt { get; set; }
}

public class PasswordResetRequest
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public User? User { get; set; }
    public string Token { get; set; } = string.Empty;
}

public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; set; }
    public User? User { get; set; }
}

// Chart Models
public class ChartBase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ChartType Type { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public DateTime ComputedAt { get; set; }
    public string Version { get; set; } = "1.0";
}

public enum ChartType
{
    Bazi,
    Astrology,
    Ziwei,
    Numerology,
    Tarot,
    HumanDesign,
    Fusion
}

public class BaziChart : ChartBase
{
    public string YearPillar { get; set; } = string.Empty;
    public string MonthPillar { get; set; } = string.Empty;
    public string DayPillar { get; set; } = string.Empty;
    public string HourPillar { get; set; } = string.Empty;
    public string DayMaster { get; set; } = string.Empty;
    public List<string> TenGods { get; set; } = new();
    public List<string> FiveElements { get; set; } = new();
    public List<LuckPeriod> LuckPeriods { get; set; } = new();
}

public class AstrologyChart : ChartBase
{
    public List<Planet> Planets { get; set; } = new();
    public List<House> Houses { get; set; } = new();
    public List<Aspect> Aspects { get; set; } = new();
    public string Ascendant { get; set; } = string.Empty;
    public string Midheaven { get; set; } = string.Empty;
}

public class Planet
{
    public string Name { get; set; } = string.Empty;
    public double Degree { get; set; }
    public string Sign { get; set; } = string.Empty;
    public int House { get; set; }
    public bool IsRetrograde { get; set; }
}

public class House
{
    public int Number { get; set; }
    public string Sign { get; set; } = string.Empty;
    public double CuspDegree { get; set; }
}

public class Aspect
{
    public string Planet1 { get; set; } = string.Empty;
    public string Planet2 { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Conjunction, Trine, etc.
    public double Orb { get; set; }
    public bool IsApplying { get; set; }
}

public class LuckPeriod
{
    public int StartAge { get; set; }
    public int EndAge { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// Personality and Psychology Models
public class PersonalityProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string MbtiType { get; set; } = string.Empty;
    public BigFiveVector BigFive { get; set; } = new();
    public int EnneagramType { get; set; }
    public string HumanDesignType { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class BigFiveVector
{
    public double Openness { get; set; }
    public double Conscientiousness { get; set; }
    public double Extraversion { get; set; }
    public double Agreeableness { get; set; }
    public double Neuroticism { get; set; }
}

// Fusion and Analysis Models
public class FusionVector
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public CareerDimension Career { get; set; } = new();
    public RelationshipDimension Relationship { get; set; } = new();
    public EnergeticBalance Energy { get; set; } = new();
    public DecisionStrategy Decision { get; set; } = new();
    public List<double> ConfidenceScores { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class CareerDimension
{
    public double CreativityDrive { get; set; }
    public double AnalyticalTendency { get; set; }
    public double LeadershipPotential { get; set; }
    public double RiskTolerance { get; set; }
    public double CollaborationStyle { get; set; }
    public List<string> SuitableIndustries { get; set; } = new();
}

public class RelationshipDimension
{
    public double AttachmentStyle { get; set; }
    public double CommunicationStyle { get; set; }
    public double ConflictResolution { get; set; }
    public double EmotionalExpression { get; set; }
    public double IntimacyPreference { get; set; }
}

public class EnergeticBalance
{
    public FiveElementBalance FiveElements { get; set; } = new();
    public AstrologicalElementBalance AstroElements { get; set; } = new();
    public string EnergyType { get; set; } = string.Empty; // From Human Design
}

public class FiveElementBalance
{
    public double Wood { get; set; }
    public double Fire { get; set; }
    public double Earth { get; set; }
    public double Metal { get; set; }
    public double Water { get; set; }
}

public class AstrologicalElementBalance
{
    public double Fire { get; set; }
    public double Earth { get; set; }
    public double Air { get; set; }
    public double Water { get; set; }
}

public class DecisionStrategy
{
    public double IntuitiveTendency { get; set; }
    public double AnalyticalTendency { get; set; }
    public double EmotionalWeight { get; set; }
    public double SocialInfluence { get; set; }
    public string PrimaryStrategy { get; set; } = string.Empty;
}

// Relationship Analysis Models
public class RelationshipReport
{
    public int Id { get; set; }
    public int UserAId { get; set; }
    public int UserBId { get; set; }
    public CompatibilityIndex Compatibility { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> Challenges { get; set; } = new();
    public List<string> GrowthAreas { get; set; } = new();
    public List<ImportantDate> ImportantDates { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CompatibilityIndex
{
    public double Overall { get; set; }
    public double Emotional { get; set; }
    public double Intellectual { get; set; }
    public double Physical { get; set; }
    public double Spiritual { get; set; }
    public double LongTermPotential { get; set; }
}

public class ImportantDate
{
    public DateTime Date { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Intensity { get; set; } // 0-1 scale
}

// Tarot and Divination Models
public class TarotSpread
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SpreadType { get; set; } = string.Empty; // ThreeCard, CelticCross, etc.
    public string Question { get; set; } = string.Empty;
    public List<TarotCard> Cards { get; set; } = new();
    public string Interpretation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TarotCard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
    public int Number { get; set; }
    public bool IsReversed { get; set; }
    public string Position { get; set; } = string.Empty; // Position in spread
    public string Meaning { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

// Timeline and Life Events Models
public class TimelinePoint
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double LoveScore { get; set; }
    public double CareerScore { get; set; }
    public double EnergyScore { get; set; }
    public double OverallScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> SourceSystems { get; set; } = new(); // Which systems contributed
    public string SourceWeightsJson { get; set; } = string.Empty;
}

// Feedback and Learning Models
public class UserFeedback
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty; // report, spread, timeline
    public int EntityId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public List<string> Tags { get; set; } = new(); // accurate, helpful, confusing, etc.
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// UI and Display Models
public class InsightItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class EnergyIndicator
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string DisplayText { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

// Daily Recommendation Models
public class RecommendationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // User, Reading, Insight, Event
    public double Score { get; set; } // Relevance score (0-100)
    public bool IsTopTen { get; set; }
    public int Rank { get; set; } // 1-based ranking
    public DateTime CreatedAt { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty; // Navigation target
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class DailyRecommendationList
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<RecommendationItem> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
}

// Purchase and Subscription Models
public class Purchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty; // pending, completed, failed
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Tier { get; set; } = string.Empty; // free, premium, expert
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string PaymentProvider { get; set; } = string.Empty;
    public string ExternalSubscriptionId { get; set; } = string.Empty;
}

// Language and Content Models
public class LocalizedContent
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

// Zodiac and Astrology Models
public enum ZodiacSign
{
    Aries, Taurus, Gemini, Cancer, Leo, Virgo,
    Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces
}

public enum ChineseZodiac
{
    Rat, Ox, Tiger, Rabbit, Dragon, Snake,
    Horse, Goat, Monkey, Rooster, Dog, Pig
}

public class ZodiacProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ZodiacSign WesternSign { get; set; }
    public ChineseZodiac ChineseSign { get; set; }
    public string RisingSign { get; set; } = string.Empty;
    public string MoonSign { get; set; } = string.Empty;
    public string Element { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string LoveStyle { get; set; } = string.Empty;
    public string CareerPath { get; set; } = string.Empty;
    public string DailyLuck { get; set; } = string.Empty;
    public string LuckyColors { get; set; } = string.Empty;
    public string LuckyNumbers { get; set; } = string.Empty;
    
    public User User { get; set; } = null!;
}

// Social Features Models
public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public int ProfileViews { get; set; }
    public double PopularityScore { get; set; }
    public string Languages { get; set; } = string.Empty; // JSON array
    public string Interests { get; set; } = string.Empty; // JSON array
    public string LookingFor { get; set; } = string.Empty; // Friends, Dating, Marriage, Business
    
    // Premium Features
    public bool IsPremium { get; set; }
    public DateTime? PremiumExpiry { get; set; }
    public string SubscriptionType { get; set; } = "Free"; // Free, Basic, Premium, VIP
    public int CreditsRemaining { get; set; }
    
    // Compatibility analysis
    [Ignore]
    public double CompatibilityScore { get; set; }
    
    public User User { get; set; } = null!;
    public ZodiacProfile? ZodiacProfile { get; set; }
}

public class CompatibilityMatch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TargetUserId { get; set; }
    public double OverallScore { get; set; }
    public double ZodiacScore { get; set; }
    public double BaziScore { get; set; }
    public double NumerologyScore { get; set; }
    public double PersonalityScore { get; set; }
    public string MatchType { get; set; } = string.Empty; // Auto, Manual, Mutual, Premium
    public string Summary { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
    public string Challenges { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public bool IsMutual { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    
    public User User { get; set; } = null!;
    public User TargetUser { get; set; } = null!;
}

public class ChatMessage
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, Chart, Reading, Gift, Sticker
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime Timestamp { get; set; }
    public string AttachmentUrl { get; set; } = string.Empty;
    public string MetaData { get; set; } = string.Empty; // JSON for additional data
    
    // For AI Assistant
    public bool IsUser { get; set; } = false;
    public bool IsAI => !IsUser;
    public bool IsReceived => !IsUser;
    
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}

public class UserInteraction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TargetUserId { get; set; }
    public string InteractionType { get; set; } = string.Empty; // Like, SuperLike, View, Block, Report, Gift
    public DateTime CreatedAt { get; set; }
    public string MetaData { get; set; } = string.Empty; // Additional interaction data
    
    public User User { get; set; } = null!;
    public User TargetUser { get; set; } = null!;
}

public class SocialFeed
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PostType { get; set; } = string.Empty; // DailyReading, Achievement, Milestone, Question
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    public User User { get; set; } = null!;
}

public class VirtualGift
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int CreditCost { get; set; }
    public string Category { get; set; } = string.Empty; // Flowers, Crystals, Charms, Premium
    public string Description { get; set; } = string.Empty;
    public bool IsLimited { get; set; }
    public DateTime? AvailableUntil { get; set; }
}

public class GiftTransaction
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int GiftId { get; set; }
    public int Quantity { get; set; }
    public int TotalCost { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
    public VirtualGift Gift { get; set; } = null!;
}
