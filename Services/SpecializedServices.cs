using KnewFate.Models;
using Newtonsoft.Json;

namespace KnewFate.Services;

// Tarot Service
public interface ITarotService
{
    Task<TarotSpread> DrawCardsAsync(string spreadType, string question, int userId);
    Task<List<TarotCard>> GetTarotDeckAsync();
    Task<string> InterpretSpreadAsync(TarotSpread spread);
    Task<TarotCard> DrawSingleCardAsync();
}

public class TarotService : ITarotService
{
    private readonly IDatabaseService _databaseService;
    private static List<TarotCard>? _tarotDeck;

    public TarotService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<TarotSpread> DrawCardsAsync(string spreadType, string question, int userId)
    {
        var deck = await GetTarotDeckAsync();
        var spread = new TarotSpread
        {
            UserId = userId,
            SpreadType = spreadType,
            Question = question,
            CreatedAt = DateTime.UtcNow,
            Cards = new List<TarotCard>()
        };

        var shuffledDeck = deck.OrderBy(x => Guid.NewGuid()).ToList();
        var cardCount = GetCardCountForSpread(spreadType);
        var positions = GetPositionsForSpread(spreadType);

        for (int i = 0; i < cardCount && i < shuffledDeck.Count; i++)
        {
            var card = shuffledDeck[i];
            card.Position = positions[i];
            card.IsReversed = Random.Shared.NextDouble() < 0.3; // 30% chance of reversal
            spread.Cards.Add(card);
        }

        spread.Interpretation = await InterpretSpreadAsync(spread);
        await _databaseService.SaveAsync(spread);

        return spread;
    }

    public async Task<List<TarotCard>> GetTarotDeckAsync()
    {
        if (_tarotDeck != null)
            return _tarotDeck;

        _tarotDeck = new List<TarotCard>();

        // Major Arcana
        var majorArcana = new[]
        {
            ("The Fool", "New beginnings, innocence, spontaneity"),
            ("The Magician", "Willpower, desire, creation, manifestation"),
            ("The High Priestess", "Intuitive, unconscious, inner voice"),
            ("The Empress", "Motherhood, fertility, nature"),
            ("The Emperor", "Authority, structure, control, father-figure"),
            ("The Hierophant", "Tradition, conformity, morality, ethics"),
            ("The Lovers", "Partnerships, duality, union"),
            ("The Chariot", "Direction, control, willpower"),
            ("Strength", "Inner strength, bravery, compassion, focus"),
            ("The Hermit", "Soul searching, introspection, inner guidance"),
            ("Wheel of Fortune", "Good luck, karma, life cycles, destiny"),
            ("Justice", "Justice, fairness, truth, cause and effect"),
            ("The Hanged Man", "Sacrifice, release, martyrdom"),
            ("Death", "Transformation, endings, change"),
            ("Temperance", "Balance, moderation, patience, purpose"),
            ("The Devil", "Bondage, addiction, sexuality, materialism"),
            ("The Tower", "Sudden upheaval, broken pride, disaster"),
            ("The Star", "Hope, faith, rejuvenation, healing"),
            ("The Moon", "Unconscious, illusions, intuition"),
            ("The Sun", "Joy, success, celebration, positivity"),
            ("Judgement", "Reflection, reckoning, awakening"),
            ("The World", "Fulfillment, harmony, completion")
        };

        for (int i = 0; i < majorArcana.Length; i++)
        {
            _tarotDeck.Add(new TarotCard
            {
                Id = i + 1,
                Name = majorArcana[i].Item1,
                Suit = "Major Arcana",
                Number = i,
                Meaning = majorArcana[i].Item2,
                ImagePath = $"tarot_{majorArcana[i].Item1.ToLower().Replace(" ", "_")}.png"
            });
        }

        // Minor Arcana
        var suits = new[] { "Wands", "Cups", "Swords", "Pentacles" };
        var suitMeanings = new Dictionary<string, string>
        {
            ["Wands"] = "Fire element, creativity, passion, career",
            ["Cups"] = "Water element, emotions, relationships, spirituality",
            ["Swords"] = "Air element, thoughts, communication, challenges",
            ["Pentacles"] = "Earth element, material world, money, health"
        };

        int cardId = 23;
        foreach (var suit in suits)
        {
            // Number cards (Ace = 1 through 10)
            for (int number = 1; number <= 10; number++)
            {
                var cardName = number == 1 ? $"Ace of {suit}" : $"{number} of {suit}";
                _tarotDeck.Add(new TarotCard
                {
                    Id = cardId++,
                    Name = cardName,
                    Suit = suit,
                    Number = number,
                    Meaning = $"{suitMeanings[suit]} - {GetNumberMeaning(number)}",
                    ImagePath = $"tarot_{suit.ToLower()}_{number}.png"
                });
            }

            // Court cards
            var courtCards = new[] { "Page", "Knight", "Queen", "King" };
            foreach (var court in courtCards)
            {
                _tarotDeck.Add(new TarotCard
                {
                    Id = cardId++,
                    Name = $"{court} of {suit}",
                    Suit = suit,
                    Number = 0, // Court cards don't have numbers
                    Meaning = $"{suitMeanings[suit]} - {GetCourtMeaning(court)}",
                    ImagePath = $"tarot_{suit.ToLower()}_{court.ToLower()}.png"
                });
            }
        }

        return _tarotDeck;
    }

    public async Task<string> InterpretSpreadAsync(TarotSpread spread)
    {
        await Task.Delay(100); // Simulate interpretation time

        var interpretation = spread.SpreadType switch
        {
            "ThreeCard" => InterpretThreeCardSpread(spread.Cards),
            "CelticCross" => InterpretCelticCrossSpread(spread.Cards),
            "SingleCard" => InterpretSingleCard(spread.Cards.First()),
            _ => "General card interpretation based on drawn cards."
        };

        return interpretation;
    }

    public async Task<TarotCard> DrawSingleCardAsync()
    {
        var deck = await GetTarotDeckAsync();
        var card = deck[Random.Shared.Next(deck.Count)];
        card.IsReversed = Random.Shared.NextDouble() < 0.3;
        return card;
    }

    private int GetCardCountForSpread(string spreadType)
    {
        return spreadType switch
        {
            "SingleCard" => 1,
            "ThreeCard" => 3,
            "CelticCross" => 10,
            "Horseshoe" => 7,
            "Cross" => 5,
            _ => 3
        };
    }

    private List<string> GetPositionsForSpread(string spreadType)
    {
        return spreadType switch
        {
            "SingleCard" => new List<string> { "Present" },
            "ThreeCard" => new List<string> { "Past", "Present", "Future" },
            "CelticCross" => new List<string> 
            { 
                "Present Situation", "Challenge/Cross", "Distant Past/Foundation", 
                "Recent Past", "Possible Outcome", "Immediate Future", 
                "Your Approach", "External Influences", "Hopes and Fears", "Final Outcome" 
            },
            _ => new List<string> { "Position 1", "Position 2", "Position 3" }
        };
    }

    private string GetNumberMeaning(int number)
    {
        return number switch
        {
            1 => "New beginnings, potential, seed of opportunity",
            2 => "Balance, partnership, duality, choices",
            3 => "Creativity, growth, collaboration, initial fulfillment",
            4 => "Stability, structure, foundation, manifestation",
            5 => "Conflict, change, challenge, instability",
            6 => "Harmony, communication, cooperation, problem-solving",
            7 => "Spirituality, introspection, inner wisdom",
            8 => "Material success, achievement, recognition",
            9 => "Completion, fulfillment, accomplishment",
            10 => "End of cycle, completion, fulfillment, new beginnings",
            _ => "Undefined meaning"
        };
    }

    private string GetCourtMeaning(string court)
    {
        return court switch
        {
            "Page" => "New energy, learning, messages, young person",
            "Knight" => "Action, adventure, impulsiveness, young adult",
            "Queen" => "Mature feminine energy, nurturing, intuitive",
            "King" => "Mature masculine energy, leadership, authority",
            _ => "Court figure"
        };
    }

    private string InterpretSingleCard(TarotCard card)
    {
        var reversal = card.IsReversed ? " (Reversed)" : "";
        return $"{card.Name}{reversal}: {card.Meaning}" +
               (card.IsReversed ? " The reversed meaning suggests internal reflection or blocked energy in this area." : "");
    }

    private string InterpretThreeCardSpread(List<TarotCard> cards)
    {
        if (cards.Count < 3) return "Insufficient cards for three-card reading.";

        var past = cards[0];
        var present = cards[1];
        var future = cards[2];

        return $"**Past ({past.Position}):** {past.Name}{(past.IsReversed ? " (Reversed)" : "")} - " +
               $"This represents the foundation or past influences that have led to your current situation. {past.Meaning}\n\n" +
               
               $"**Present ({present.Position}):** {present.Name}{(present.IsReversed ? " (Reversed)" : "")} - " +
               $"This reflects your current circumstances and the energy surrounding you now. {present.Meaning}\n\n" +
               
               $"**Future ({future.Position}):** {future.Name}{(future.IsReversed ? " (Reversed)" : "")} - " +
               $"This indicates the likely outcome or path forward based on current energies. {future.Meaning}\n\n" +
               
               "Remember, the future is not set in stone and can be influenced by your choices and actions.";
    }

    private string InterpretCelticCrossSpread(List<TarotCard> cards)
    {
        if (cards.Count < 10) return "Insufficient cards for Celtic Cross reading.";

        var interpretation = "**Celtic Cross Reading:**\n\n";
        
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            interpretation += $"**{card.Position}:** {card.Name}{(card.IsReversed ? " (Reversed)" : "")} - {card.Meaning}\n\n";
        }

        interpretation += "This comprehensive spread reveals the complex interplay of forces in your situation. " +
                         "Pay special attention to the relationships between the cards and how they tell your story.";

        return interpretation;
    }
}

// Astrology Service
public interface IAstrologyService
{
    Task<AstrologyChart> CalculateNatalChartAsync(BirthData birthData);
    Task<List<string>> GetCurrentTransitsAsync(AstrologyChart natalChart);
    Task<string> GenerateAstrologyReportAsync(AstrologyChart chart);
    Task<Dictionary<string, double>> CalculateCompatibilityAsync(AstrologyChart chart1, AstrologyChart chart2);
}

public class AstrologyService : IAstrologyService
{
    private readonly IChartCalculationService _chartCalculationService;

    public AstrologyService(IChartCalculationService chartCalculationService)
    {
        _chartCalculationService = chartCalculationService;
    }

    public async Task<AstrologyChart> CalculateNatalChartAsync(BirthData birthData)
    {
        return await _chartCalculationService.CalculateAstrologyChartAsync(birthData);
    }

    public async Task<List<string>> GetCurrentTransitsAsync(AstrologyChart natalChart)
    {
        await Task.Delay(100);
        
        // Mock current transits
        var transits = new List<string>
        {
            "Jupiter trine natal Sun - Expansion and growth opportunities",
            "Saturn square natal Moon - Emotional restructuring and maturity",
            "Mercury conjunct natal Venus - Enhanced communication in relationships",
            "Mars opposite natal Mars - Energy conflicts and competitive challenges"
        };

        return transits;
    }

    public async Task<string> GenerateAstrologyReportAsync(AstrologyChart chart)
    {
        await Task.Delay(200);

        var report = "**Your Astrological Profile**\n\n";
        
        // Sun sign analysis
        var sun = chart.Planets.FirstOrDefault(p => p.Name == "Sun");
        if (sun != null)
        {
            report += $"**Sun in {sun.Sign} (House {sun.House}):** " +
                     $"Your core identity and life purpose. {GetSignMeaning(sun.Sign)} " +
                     $"In the {GetOrdinal(sun.House)} house, this energy expresses through {GetHouseMeaning(sun.House)}.\n\n";
        }

        // Moon sign analysis
        var moon = chart.Planets.FirstOrDefault(p => p.Name == "Moon");
        if (moon != null)
        {
            report += $"**Moon in {moon.Sign} (House {moon.House}):** " +
                     $"Your emotional nature and inner world. {GetSignMeaning(moon.Sign)} " +
                     $"This emotional energy manifests in the {GetOrdinal(moon.House)} house of {GetHouseMeaning(moon.House)}.\n\n";
        }

        // Ascendant analysis
        report += $"**Ascendant in {chart.Ascendant}:** " +
                 $"Your outer personality and how others see you. {GetSignMeaning(chart.Ascendant)}\n\n";

        // Key aspects
        if (chart.Aspects.Any())
        {
            report += "**Key Planetary Aspects:**\n";
            foreach (var aspect in chart.Aspects.Take(3))
            {
                report += $"• {aspect.Planet1} {aspect.Type} {aspect.Planet2}: {GetAspectMeaning(aspect.Type)}\n";
            }
        }

        return report;
    }

    public async Task<Dictionary<string, double>> CalculateCompatibilityAsync(AstrologyChart chart1, AstrologyChart chart2)
    {
        await Task.Delay(150);

        // Mock compatibility calculation
        var compatibility = new Dictionary<string, double>
        {
            ["Overall"] = 0.75,
            ["Emotional"] = 0.80,
            ["Communication"] = 0.70,
            ["Values"] = 0.65,
            ["Physical"] = 0.85,
            ["LongTerm"] = 0.72
        };

        return compatibility;
    }

    private string GetSignMeaning(string sign)
    {
        return sign switch
        {
            "Aries" => "Dynamic, pioneering, and enthusiastic energy.",
            "Taurus" => "Stable, practical, and sensual nature.",
            "Gemini" => "Curious, communicative, and adaptable personality.",
            "Cancer" => "Nurturing, intuitive, and emotionally sensitive.",
            "Leo" => "Creative, confident, and naturally leadership-oriented.",
            "Virgo" => "Analytical, helpful, and detail-oriented approach.",
            "Libra" => "Harmonious, diplomatic, and relationship-focused.",
            "Scorpio" => "Intense, transformative, and deeply perceptive.",
            "Sagittarius" => "Adventurous, philosophical, and freedom-loving.",
            "Capricorn" => "Ambitious, disciplined, and goal-oriented.",
            "Aquarius" => "Independent, innovative, and humanitarian.",
            "Pisces" => "Compassionate, intuitive, and spiritually inclined.",
            _ => "Balanced and harmonious energy."
        };
    }

    private string GetHouseMeaning(int house)
    {
        return house switch
        {
            1 => "self-identity and personal appearance",
            2 => "values, possessions, and self-worth",
            3 => "communication, siblings, and short journeys",
            4 => "home, family, and emotional foundations",
            5 => "creativity, romance, and self-expression",
            6 => "work, health, and daily routines",
            7 => "partnerships and one-on-one relationships",
            8 => "transformation, shared resources, and deep psychology",
            9 => "philosophy, higher learning, and long journeys",
            10 => "career, reputation, and public image",
            11 => "friendships, groups, and future aspirations",
            12 => "spirituality, subconscious, and hidden matters",
            _ => "various life areas"
        };
    }

    private string GetAspectMeaning(string aspectType)
    {
        return aspectType switch
        {
            "Conjunction" => "Powerful blending of planetary energies",
            "Trine" => "Harmonious flow and natural talents",
            "Sextile" => "Opportunities for growth and cooperation",
            "Square" => "Dynamic tension requiring conscious work",
            "Opposition" => "Balancing and integration of opposing forces",
            _ => "Planetary interaction"
        };
    }

    private string GetOrdinal(int number)
    {
        return number switch
        {
            1 => "1st", 2 => "2nd", 3 => "3rd", 4 => "4th", 5 => "5th", 6 => "6th",
            7 => "7th", 8 => "8th", 9 => "9th", 10 => "10th", 11 => "11th", 12 => "12th",
            _ => $"{number}th"
        };
    }
}

// BaZi (Four Pillars) Service
public interface IBaziService
{
    Task<BaziChart> CalculateBaziAsync(BirthData birthData);
    Task<string> AnalyzeTenGodsAsync(BaziChart chart);
    Task<string> AnalyzeFiveElementsAsync(BaziChart chart);
    Task<List<LuckPeriod>> CalculateLuckPeriodsAsync(BaziChart chart);
    Task<string> GenerateBaziReportAsync(BaziChart chart);
}

public class BaziService : IBaziService
{
    private readonly IChartCalculationService _chartCalculationService;

    public BaziService(IChartCalculationService chartCalculationService)
    {
        _chartCalculationService = chartCalculationService;
    }

    public async Task<BaziChart> CalculateBaziAsync(BirthData birthData)
    {
        return await _chartCalculationService.CalculateBaziChartAsync(birthData);
    }

    public async Task<string> AnalyzeTenGodsAsync(BaziChart chart)
    {
        await Task.Delay(100);

        var analysis = "**Ten Gods Analysis:**\n\n";
        
        foreach (var god in chart.TenGods)
        {
            analysis += $"**{god}:** {GetTenGodMeaning(god)}\n\n";
        }

        return analysis;
    }

    public async Task<string> AnalyzeFiveElementsAsync(BaziChart chart)
    {
        await Task.Delay(100);

        var analysis = "**Five Elements Balance:**\n\n";
        
        foreach (var element in chart.FiveElements)
        {
            analysis += $"**{element}:** {GetElementMeaning(element)}\n\n";
        }

        return analysis;
    }

    public async Task<List<LuckPeriod>> CalculateLuckPeriodsAsync(BaziChart chart)
    {
        await Task.Delay(100);
        return chart.LuckPeriods;
    }

    public async Task<string> GenerateBaziReportAsync(BaziChart chart)
    {
        await Task.Delay(200);

        var report = "**BaZi (Four Pillars) Analysis**\n\n";
        
        report += $"**Four Pillars:**\n";
        report += $"Year: {chart.YearPillar} | Month: {chart.MonthPillar} | Day: {chart.DayPillar} | Hour: {chart.HourPillar}\n\n";
        
        report += $"**Day Master:** {chart.DayMaster}\n";
        report += $"Your Day Master represents your core essence and represents {GetDayMasterMeaning(chart.DayMaster)}.\n\n";

        report += await AnalyzeTenGodsAsync(chart);
        report += await AnalyzeFiveElementsAsync(chart);

        if (chart.LuckPeriods.Any())
        {
            report += "**Major Luck Periods:**\n";
            foreach (var period in chart.LuckPeriods.Take(3))
            {
                report += $"Ages {period.StartAge}-{period.EndAge}: {period.Stem}{period.Branch} - {period.Description}\n";
            }
        }

        return report;
    }

    private string GetTenGodMeaning(string god)
    {
        return god switch
        {
            "正官" => "Proper authority, leadership, discipline, and social responsibility",
            "偏官" => "Unconventional power, challenge, pressure, and transformation",
            "正印" => "Traditional knowledge, nurturing, support, and academic pursuits",
            "偏印" => "Creative thinking, alternative learning, and unique perspectives",
            "比肩" => "Peers, cooperation, stubbornness, and self-reliance",
            "劫财" => "Competition, impulsiveness, risk-taking, and material desires",
            "食神" => "Creativity, communication, offspring, and artistic expression",
            "伤官" => "Innovation, rebellion, talent, and non-conformity",
            "正财" => "Proper wealth, management, stability, and traditional values",
            "偏财" => "Windfall, speculation, charm, and unexpected opportunities",
            _ => "Balanced energy representing various life aspects"
        };
    }

    private string GetElementMeaning(string element)
    {
        return element switch
        {
            "木" => "Growth, flexibility, planning, and creative expansion",
            "火" => "Passion, communication, activity, and bright energy",
            "土" => "Stability, reliability, nurturing, and practical grounding",
            "金" => "Structure, precision, determination, and refined judgment",
            "水" => "Wisdom, adaptability, flow, and deep intelligence",
            _ => "Balanced elemental energy"
        };
    }

    private string GetDayMasterMeaning(string dayMaster)
    {
        return dayMaster switch
        {
            "甲" => "Yang Wood - Strong leadership, pioneering spirit, and natural growth",
            "乙" => "Yin Wood - Gentle flexibility, adaptability, and artistic nature",
            "丙" => "Yang Fire - Bright enthusiasm, communication skills, and warm personality",
            "丁" => "Yin Fire - Refined culture, attention to detail, and careful planning",
            "戊" => "Yang Earth - Steady reliability, practical approach, and strong foundation",
            "己" => "Yin Earth - Nurturing care, supportive nature, and patient cultivation",
            "庚" => "Yang Metal - Sharp determination, clear thinking, and strong will",
            "辛" => "Yin Metal - Precious refinement, aesthetic sense, and careful craftsmanship",
            "壬" => "Yang Water - Flowing wisdom, broad perspective, and natural intelligence",
            "癸" => "Yin Water - Deep intuition, quiet strength, and profound understanding",
            _ => "Balanced personality with multiple facets"
        };
    }
}

// Relationship Compatibility Service
public interface IRelationshipService
{
    Task<RelationshipReport> AnalyzeCompatibilityAsync(int userAId, int userBId);
    Task<List<ImportantDate>> GetRelationshipTimelineAsync(RelationshipReport report);
    Task<string> GenerateRelationshipAdviceAsync(RelationshipReport report);
}

public class RelationshipService : IRelationshipService
{
    private readonly IDatabaseService _databaseService;
    private readonly IAstrologyService _astrologyService;
    private readonly IBaziService _baziService;
    private readonly IFusionEngineService _fusionEngineService;

    public RelationshipService(
        IDatabaseService databaseService,
        IAstrologyService astrologyService,
        IBaziService baziService,
        IFusionEngineService fusionEngineService)
    {
        _databaseService = databaseService;
        _astrologyService = astrologyService;
        _baziService = baziService;
        _fusionEngineService = fusionEngineService;
    }

    public async Task<RelationshipReport> AnalyzeCompatibilityAsync(int userAId, int userBId)
    {
        // Get birth data for both users
        var userAData = await _databaseService.QueryAsync<BirthData>("SELECT * FROM BirthData WHERE UserId = ?", userAId);
        var userBData = await _databaseService.QueryAsync<BirthData>("SELECT * FROM BirthData WHERE UserId = ?", userBId);

        if (!userAData.Any() || !userBData.Any())
        {
            throw new ArgumentException("Birth data not found for one or both users");
        }

        // Calculate charts for both users
        var chartA = await _astrologyService.CalculateNatalChartAsync(userAData.First());
        var chartB = await _astrologyService.CalculateNatalChartAsync(userBData.First());

        // Calculate compatibility
        var astroCompatibility = await _astrologyService.CalculateCompatibilityAsync(chartA, chartB);

        // Mock BaZi compatibility (would be calculated from actual charts)
        var baziCompatibility = new Dictionary<string, double>
        {
            ["ElementalHarmony"] = 0.70,
            ["TenGodsSynergy"] = 0.65,
            ["LuckPeriodAlignment"] = 0.75
        };

        var report = new RelationshipReport
        {
            UserAId = userAId,
            UserBId = userBId,
            Compatibility = new CompatibilityIndex
            {
                Overall = (astroCompatibility["Overall"] + baziCompatibility["ElementalHarmony"]) / 2,
                Emotional = astroCompatibility["Emotional"],
                Intellectual = astroCompatibility["Communication"],
                Physical = astroCompatibility["Physical"],
                Spiritual = baziCompatibility["ElementalHarmony"],
                LongTermPotential = astroCompatibility["LongTerm"]
            },
            Strengths = await GenerateStrengths(astroCompatibility, baziCompatibility),
            Challenges = await GenerateChallenges(astroCompatibility, baziCompatibility),
            GrowthAreas = await GenerateGrowthAreas(astroCompatibility, baziCompatibility),
            CreatedAt = DateTime.UtcNow
        };

        report.ImportantDates = await GetRelationshipTimelineAsync(report);
        await _databaseService.SaveAsync(report);

        return report;
    }

    public async Task<List<ImportantDate>> GetRelationshipTimelineAsync(RelationshipReport report)
    {
        await Task.Delay(100);

        var importantDates = new List<ImportantDate>
        {
            new()
            {
                Date = DateTime.Now.AddMonths(2),
                EventType = "Communication Enhancement",
                Description = "Mercury aspects suggest improved understanding between partners",
                Intensity = 0.7
            },
            new()
            {
                Date = DateTime.Now.AddMonths(6),
                EventType = "Relationship Deepening",
                Description = "Venus conjunction indicates stronger emotional bonding",
                Intensity = 0.8
            },
            new()
            {
                Date = DateTime.Now.AddMonths(12),
                EventType = "Major Decision Point",
                Description = "Saturn aspects suggest important relationship choices",
                Intensity = 0.9
            }
        };

        return importantDates;
    }

    public async Task<string> GenerateRelationshipAdviceAsync(RelationshipReport report)
    {
        await Task.Delay(100);

        var advice = "**Relationship Guidance:**\n\n";
        
        advice += $"**Overall Compatibility: {report.Compatibility.Overall:P0}**\n\n";
        
        advice += "**Your Strengths Together:**\n";
        foreach (var strength in report.Strengths)
        {
            advice += $"• {strength}\n";
        }
        
        advice += "\n**Areas for Growth:**\n";
        foreach (var growth in report.GrowthAreas)
        {
            advice += $"• {growth}\n";
        }

        advice += "\n**Challenges to Navigate:**\n";
        foreach (var challenge in report.Challenges)
        {
            advice += $"• {challenge}\n";
        }

        return advice;
    }

    private async Task<List<string>> GenerateStrengths(Dictionary<string, double> astro, Dictionary<string, double> bazi)
    {
        await Task.Delay(50);
        
        var strengths = new List<string>();
        
        if (astro["Emotional"] > 0.7)
            strengths.Add("Strong emotional connection and understanding");
        
        if (astro["Communication"] > 0.7)
            strengths.Add("Excellent communication and intellectual rapport");
            
        if (bazi["ElementalHarmony"] > 0.7)
            strengths.Add("Harmonious energy balance supporting long-term stability");

        if (astro["Physical"] > 0.8)
            strengths.Add("Strong physical and romantic chemistry");

        return strengths;
    }

    private async Task<List<string>> GenerateChallenges(Dictionary<string, double> astro, Dictionary<string, double> bazi)
    {
        await Task.Delay(50);
        
        var challenges = new List<string>();
        
        if (astro["Communication"] < 0.6)
            challenges.Add("Communication styles may require conscious effort to harmonize");
        
        if (bazi["TenGodsSynergy"] < 0.6)
            challenges.Add("Different approaches to authority and life goals need discussion");
            
        if (astro["LongTerm"] < 0.7)
            challenges.Add("Long-term compatibility requires ongoing commitment and growth");

        return challenges;
    }

    private async Task<List<string>> GenerateGrowthAreas(Dictionary<string, double> astro, Dictionary<string, double> bazi)
    {
        await Task.Delay(50);
        
        var growthAreas = new List<string>
        {
            "Practice active listening and express appreciation regularly",
            "Create shared goals and vision for your future together",
            "Respect each other's individual growth and personal space",
            "Develop conflict resolution skills and healthy boundaries"
        };

        return growthAreas;
    }
}
