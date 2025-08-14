using KnewFate.Models;

namespace KnewFate.Services;

public interface IZodiacService
{
    Task<ZodiacProfile> GetZodiacProfileAsync(int userId);
    Task<ZodiacProfile> CreateZodiacProfileAsync(int userId, DateTime birthDate, TimeSpan birthTime, string birthPlace);
    Task<string> GetDailyHoroscopeAsync(ZodiacSign sign, string language = "en");
    Task<string> GetWeeklyHoroscopeAsync(ZodiacSign sign, string language = "en");
    Task<string> GetMonthlyHoroscopeAsync(ZodiacSign sign, string language = "en");
    Task<List<string>> GetLuckyColorsAsync(ZodiacSign sign);
    Task<List<int>> GetLuckyNumbersAsync(ZodiacSign sign);
    Task<double> CalculateZodiacCompatibilityAsync(ZodiacSign sign1, ZodiacSign sign2);
    Task<ZodiacSign> GetZodiacSignFromDateAsync(DateTime birthDate);
    Task<ChineseZodiac> GetChineseZodiacFromDateAsync(DateTime birthDate);
}

public class ZodiacService : IZodiacService
{
    private readonly IDatabaseService _databaseService;
    private readonly ILocalizationService _localizationService;

    public ZodiacService(IDatabaseService databaseService, ILocalizationService localizationService)
    {
        _databaseService = databaseService;
        _localizationService = localizationService;
    }

    public async Task<ZodiacProfile> GetZodiacProfileAsync(int userId)
    {
        var db = await _databaseService.GetDatabaseAsync();
        return await db.Table<ZodiacProfile>()
            .Where(z => z.UserId == userId)
            .FirstOrDefaultAsync() ?? new ZodiacProfile { UserId = userId };
    }

    public async Task<ZodiacProfile> CreateZodiacProfileAsync(int userId, DateTime birthDate, TimeSpan birthTime, string birthPlace)
    {
        var westernSign = await GetZodiacSignFromDateAsync(birthDate);
        var chineseSign = await GetChineseZodiacFromDateAsync(birthDate);
        
        var profile = new ZodiacProfile
        {
            UserId = userId,
            WesternSign = westernSign,
            ChineseSign = chineseSign,
            Element = GetElementForSign(westernSign),
            Personality = await GetPersonalityDescriptionAsync(westernSign),
            Strengths = await GetStrengthsAsync(westernSign),
            Weaknesses = await GetWeaknessesAsync(westernSign),
            LoveStyle = await GetLoveStyleAsync(westernSign),
            CareerPath = await GetCareerPathAsync(westernSign),
            LuckyColors = string.Join(",", await GetLuckyColorsAsync(westernSign)),
            LuckyNumbers = string.Join(",", (await GetLuckyNumbersAsync(westernSign)).Select(n => n.ToString()))
        };

        var db = await _databaseService.GetDatabaseAsync();
        await db.InsertOrReplaceAsync(profile);
        
        return profile;
    }

    public async Task<string> GetDailyHoroscopeAsync(ZodiacSign sign, string language = "en")
    {
        // Generate daily horoscope based on current planetary positions
        var horoscopes = GetDailyHoroscopes(language);
        return horoscopes.ContainsKey(sign) ? horoscopes[sign] : "今日运势良好，保持积极心态。";
    }

    public async Task<string> GetWeeklyHoroscopeAsync(ZodiacSign sign, string language = "en")
    {
        var horoscopes = GetWeeklyHoroscopes(language);
        return horoscopes.ContainsKey(sign) ? horoscopes[sign] : "本周运势稳定，适合规划未来。";
    }

    public async Task<string> GetMonthlyHoroscopeAsync(ZodiacSign sign, string language = "en")
    {
        var horoscopes = GetMonthlyHoroscopes(language);
        return horoscopes.ContainsKey(sign) ? horoscopes[sign] : "本月运势向好，把握机会。";
    }

    public async Task<List<string>> GetLuckyColorsAsync(ZodiacSign sign)
    {
        return sign switch
        {
            ZodiacSign.Aries => new List<string> { "Red", "Orange", "Coral" },
            ZodiacSign.Taurus => new List<string> { "Green", "Pink", "Brown" },
            ZodiacSign.Gemini => new List<string> { "Yellow", "Silver", "Light Blue" },
            ZodiacSign.Cancer => new List<string> { "White", "Silver", "Sea Green" },
            ZodiacSign.Leo => new List<string> { "Gold", "Orange", "Purple" },
            ZodiacSign.Virgo => new List<string> { "Navy Blue", "Gray", "Beige" },
            ZodiacSign.Libra => new List<string> { "Pink", "Light Blue", "Lavender" },
            ZodiacSign.Scorpio => new List<string> { "Dark Red", "Black", "Maroon" },
            ZodiacSign.Sagittarius => new List<string> { "Purple", "Turquoise", "Orange" },
            ZodiacSign.Capricorn => new List<string> { "Black", "Brown", "Dark Green" },
            ZodiacSign.Aquarius => new List<string> { "Blue", "Silver", "Aqua" },
            ZodiacSign.Pisces => new List<string> { "Sea Green", "Lilac", "White" },
            _ => new List<string> { "Blue", "White", "Silver" }
        };
    }

    public async Task<List<int>> GetLuckyNumbersAsync(ZodiacSign sign)
    {
        return sign switch
        {
            ZodiacSign.Aries => new List<int> { 1, 8, 17 },
            ZodiacSign.Taurus => new List<int> { 2, 6, 9, 12, 24 },
            ZodiacSign.Gemini => new List<int> { 5, 7, 14, 23 },
            ZodiacSign.Cancer => new List<int> { 2, 7, 11, 16, 20, 25 },
            ZodiacSign.Leo => new List<int> { 1, 3, 10, 19 },
            ZodiacSign.Virgo => new List<int> { 3, 15, 20, 27 },
            ZodiacSign.Libra => new List<int> { 4, 6, 13, 15, 24 },
            ZodiacSign.Scorpio => new List<int> { 8, 11, 18, 22 },
            ZodiacSign.Sagittarius => new List<int> { 3, 9, 12, 21 },
            ZodiacSign.Capricorn => new List<int> { 6, 8, 10, 26 },
            ZodiacSign.Aquarius => new List<int> { 4, 7, 11, 22, 29 },
            ZodiacSign.Pisces => new List<int> { 3, 9, 12, 15, 18, 24 },
            _ => new List<int> { 7, 14, 21 }
        };
    }

    public async Task<double> CalculateZodiacCompatibilityAsync(ZodiacSign sign1, ZodiacSign sign2)
    {
        var compatibilityMatrix = GetCompatibilityMatrix();
        var key = $"{sign1}_{sign2}";
        var reverseKey = $"{sign2}_{sign1}";
        
        if (compatibilityMatrix.ContainsKey(key))
            return compatibilityMatrix[key];
        else if (compatibilityMatrix.ContainsKey(reverseKey))
            return compatibilityMatrix[reverseKey];
        
        return 0.5; // Default compatibility
    }

    public async Task<ZodiacSign> GetZodiacSignFromDateAsync(DateTime birthDate)
    {
        var month = birthDate.Month;
        var day = birthDate.Day;

        return (month, day) switch
        {
            var date when (month == 3 && day >= 21) || (month == 4 && day <= 19) => ZodiacSign.Aries,
            var date when (month == 4 && day >= 20) || (month == 5 && day <= 20) => ZodiacSign.Taurus,
            var date when (month == 5 && day >= 21) || (month == 6 && day <= 20) => ZodiacSign.Gemini,
            var date when (month == 6 && day >= 21) || (month == 7 && day <= 22) => ZodiacSign.Cancer,
            var date when (month == 7 && day >= 23) || (month == 8 && day <= 22) => ZodiacSign.Leo,
            var date when (month == 8 && day >= 23) || (month == 9 && day <= 22) => ZodiacSign.Virgo,
            var date when (month == 9 && day >= 23) || (month == 10 && day <= 22) => ZodiacSign.Libra,
            var date when (month == 10 && day >= 23) || (month == 11 && day <= 21) => ZodiacSign.Scorpio,
            var date when (month == 11 && day >= 22) || (month == 12 && day <= 21) => ZodiacSign.Sagittarius,
            var date when (month == 12 && day >= 22) || (month == 1 && day <= 19) => ZodiacSign.Capricorn,
            var date when (month == 1 && day >= 20) || (month == 2 && day <= 18) => ZodiacSign.Aquarius,
            var date when (month == 2 && day >= 19) || (month == 3 && day <= 20) => ZodiacSign.Pisces,
            _ => ZodiacSign.Aries
        };
    }

    public async Task<ChineseZodiac> GetChineseZodiacFromDateAsync(DateTime birthDate)
    {
        var year = birthDate.Year;
        var zodiacIndex = (year - 1900) % 12;
        
        return zodiacIndex switch
        {
            0 => ChineseZodiac.Rat,
            1 => ChineseZodiac.Ox,
            2 => ChineseZodiac.Tiger,
            3 => ChineseZodiac.Rabbit,
            4 => ChineseZodiac.Dragon,
            5 => ChineseZodiac.Snake,
            6 => ChineseZodiac.Horse,
            7 => ChineseZodiac.Goat,
            8 => ChineseZodiac.Monkey,
            9 => ChineseZodiac.Rooster,
            10 => ChineseZodiac.Dog,
            11 => ChineseZodiac.Pig,
            _ => ChineseZodiac.Rat
        };
    }

    private string GetElementForSign(ZodiacSign sign)
    {
        return sign switch
        {
            ZodiacSign.Aries or ZodiacSign.Leo or ZodiacSign.Sagittarius => "Fire",
            ZodiacSign.Taurus or ZodiacSign.Virgo or ZodiacSign.Capricorn => "Earth",
            ZodiacSign.Gemini or ZodiacSign.Libra or ZodiacSign.Aquarius => "Air",
            ZodiacSign.Cancer or ZodiacSign.Scorpio or ZodiacSign.Pisces => "Water",
            _ => "Unknown"
        };
    }

    private async Task<string> GetPersonalityDescriptionAsync(ZodiacSign sign)
    {
        var descriptions = new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = "充满活力，勇敢直接，天生的领导者。喜欢挑战和冒险，行动力强。",
            [ZodiacSign.Taurus] = "稳重可靠，注重安全感，享受生活的美好。固执但忠诚，有很强的耐心。",
            [ZodiacSign.Gemini] = "聪明机智，善于沟通，好奇心强。适应能力强，但有时缺乏专注力。",
            [ZodiacSign.Cancer] = "敏感体贴，重视家庭，情感丰富。保护欲强，但有时过于敏感。",
            [ZodiacSign.Leo] = "自信大方，有表演天赋，喜欢成为焦点。慷慨大方，但有时过于自我。",
            [ZodiacSign.Virgo] = "细心谨慎，追求完美，注重细节。实用主义者，但有时过于挑剔。",
            [ZodiacSign.Libra] = "优雅和谐，追求平衡，有很好的审美观。善于交际，但有时优柔寡断。",
            [ZodiacSign.Scorpio] = "神秘深刻，意志坚强，洞察力强。忠诚专一，但有时过于极端。",
            [ZodiacSign.Sagittarius] = "乐观开朗，热爱自由，富有冒险精神。哲学思维，但有时缺乏耐心。",
            [ZodiacSign.Capricorn] = "务实勤奋，有强烈的责任感，追求成功。坚持不懈，但有时过于严肃。",
            [ZodiacSign.Aquarius] = "独立创新，思维前卫，富有人道主义精神。独特个性，但有时显得疏离。",
            [ZodiacSign.Pisces] = "富有想象力，敏感浪漫，直觉力强。富有同情心，但有时过于理想化。"
        };

        return descriptions.ContainsKey(sign) ? descriptions[sign] : "独特的个性，有自己的魅力。";
    }

    private async Task<string> GetStrengthsAsync(ZodiacSign sign)
    {
        var strengths = new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = "勇敢、积极、领导力强、行动迅速",
            [ZodiacSign.Taurus] = "可靠、耐心、实用、忠诚",
            [ZodiacSign.Gemini] = "聪明、适应性强、沟通能力强、学习能力快",
            [ZodiacSign.Cancer] = "体贴、保护欲强、直觉敏锐、忠诚",
            [ZodiacSign.Leo] = "自信、慷慨、有魅力、创造力强",
            [ZodiacSign.Virgo] = "细心、分析能力强、可靠、勤奋",
            [ZodiacSign.Libra] = "和谐、公正、外交能力强、审美观好",
            [ZodiacSign.Scorpio] = "坚强、专注、洞察力强、忠诚",
            [ZodiacSign.Sagittarius] = "乐观、诚实、哲学思维、冒险精神",
            [ZodiacSign.Capricorn] = "有野心、坚持不懈、负责任、实际",
            [ZodiacSign.Aquarius] = "独立、创新、人道主义、独特",
            [ZodiacSign.Pisces] = "富有同情心、直觉强、艺术天赋、适应性强"
        };

        return strengths.ContainsKey(sign) ? strengths[sign] : "独特的优点等待发掘";
    }

    private async Task<string> GetWeaknessesAsync(ZodiacSign sign)
    {
        var weaknesses = new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = "冲动、缺乏耐心、可能过于自我",
            [ZodiacSign.Taurus] = "固执、抗拒变化、有时过于物质主义",
            [ZodiacSign.Gemini] = "不专一、表面化、容易焦虑",
            [ZodiacSign.Cancer] = "过于敏感、情绪化、容易受伤",
            [ZodiacSign.Leo] = "自负、需要关注、有时专制",
            [ZodiacSign.Virgo] = "过于挑剔、担心过度、完美主义",
            [ZodiacSign.Libra] = "优柔寡断、避免冲突、有时肤浅",
            [ZodiacSign.Scorpio] = "嫉妒、报复心强、有时过于极端",
            [ZodiacSign.Sagittarius] = "不耐烦、过于直率、缺乏专注",
            [ZodiacSign.Capricorn] = "过于严肃、悲观、工作狂",
            [ZodiacSign.Aquarius] = "疏离、固执己见、有时冷漠",
            [ZodiacSign.Pisces] = "逃避现实、过于理想化、容易被影响"
        };

        return weaknesses.ContainsKey(sign) ? weaknesses[sign] : "需要注意的特质";
    }

    private async Task<string> GetLoveStyleAsync(ZodiacSign sign)
    {
        var loveStyles = new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = "热情直接，喜欢主动追求，需要激情和挑战",
            [ZodiacSign.Taurus] = "稳定忠诚，重视安全感，喜欢浪漫和舒适",
            [ZodiacSign.Gemini] = "需要智力刺激，喜欢变化，重视沟通",
            [ZodiacSign.Cancer] = "深情专一，重视家庭，需要情感安全",
            [ZodiacSign.Leo] = "浪漫大方，需要赞美，喜欢成为伴侣的骄傲",
            [ZodiacSign.Virgo] = "实用主义，重视细节，慢热但深情",
            [ZodiacSign.Libra] = "追求和谐，重视美感，需要平衡的关系",
            [ZodiacSign.Scorpio] = "深情专一，占有欲强，追求深度连接",
            [ZodiacSign.Sagittarius] = "自由开放，需要空间，喜欢冒险的伴侣",
            [ZodiacSign.Capricorn] = "认真负责，重视承诺，慢热但持久",
            [ZodiacSign.Aquarius] = "独立自主，需要友谊基础，重视精神连接",
            [ZodiacSign.Pisces] = "浪漫理想，敏感体贴，容易付出一切"
        };

        return loveStyles.ContainsKey(sign) ? loveStyles[sign] : "独特的爱情观";
    }

    private async Task<string> GetCareerPathAsync(ZodiacSign sign)
    {
        var careerPaths = new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = "领导、销售、体育、军事、创业",
            [ZodiacSign.Taurus] = "金融、艺术、美食、房地产、园艺",
            [ZodiacSign.Gemini] = "媒体、教育、写作、销售、翻译",
            [ZodiacSign.Cancer] = "护理、教育、餐饮、房地产、心理咨询",
            [ZodiacSign.Leo] = "娱乐、表演、管理、设计、公关",
            [ZodiacSign.Virgo] = "医疗、会计、编辑、服务、研究",
            [ZodiacSign.Libra] = "法律、外交、艺术、美容、咨询",
            [ZodiacSign.Scorpio] = "调查、心理学、医学、金融、研究",
            [ZodiacSign.Sagittarius] = "教育、旅游、出版、哲学、体育",
            [ZodiacSign.Capricorn] = "管理、政府、工程、银行、建筑",
            [ZodiacSign.Aquarius] = "科技、社会工作、发明、改革、研究",
            [ZodiacSign.Pisces] = "艺术、音乐、护理、宗教、慈善"
        };

        return careerPaths.ContainsKey(sign) ? careerPaths[sign] : "多元化的职业选择";
    }

    private Dictionary<ZodiacSign, string> GetDailyHoroscopes(string language)
    {
        // This would typically come from an API or database
        return new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = language == "zh-CN" ? "今日适合主动出击，把握机会。" : "Take initiative today and seize opportunities.",
            [ZodiacSign.Taurus] = language == "zh-CN" ? "保持稳定步调，避免急躁。" : "Maintain steady pace, avoid rushing.",
            [ZodiacSign.Gemini] = language == "zh-CN" ? "多与他人交流，获得新信息。" : "Communicate more with others for new insights.",
            [ZodiacSign.Cancer] = language == "zh-CN" ? "关注家庭和情感需求。" : "Focus on family and emotional needs.",
            [ZodiacSign.Leo] = language == "zh-CN" ? "展现你的才华和领导力。" : "Showcase your talents and leadership.",
            [ZodiacSign.Virgo] = language == "zh-CN" ? "注意细节，完善计划。" : "Pay attention to details and refine plans.",
            [ZodiacSign.Libra] = language == "zh-CN" ? "寻求平衡，避免冲突。" : "Seek balance and avoid conflicts.",
            [ZodiacSign.Scorpio] = language == "zh-CN" ? "深入思考，探索真相。" : "Think deeply and explore the truth.",
            [ZodiacSign.Sagittarius] = language == "zh-CN" ? "拓展视野，学习新知识。" : "Expand horizons and learn new things.",
            [ZodiacSign.Capricorn] = language == "zh-CN" ? "专注目标，坚持努力。" : "Focus on goals and persist in efforts.",
            [ZodiacSign.Aquarius] = language == "zh-CN" ? "创新思维，尝试新方法。" : "Think innovatively and try new approaches.",
            [ZodiacSign.Pisces] = language == "zh-CN" ? "相信直觉，发挥想象力。" : "Trust intuition and use imagination."
        };
    }

    private Dictionary<ZodiacSign, string> GetWeeklyHoroscopes(string language)
    {
        return new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = language == "zh-CN" ? "本周事业运势上升，适合新的开始。" : "Career prospects rise this week, good for new beginnings.",
            [ZodiacSign.Taurus] = language == "zh-CN" ? "财运稳定，但要注意健康。" : "Financial luck is stable, but pay attention to health.",
            // Add more weekly horoscopes...
        };
    }

    private Dictionary<ZodiacSign, string> GetMonthlyHoroscopes(string language)
    {
        return new Dictionary<ZodiacSign, string>
        {
            [ZodiacSign.Aries] = language == "zh-CN" ? "本月整体运势向好，有重要机会出现。" : "Overall luck improves this month with important opportunities.",
            [ZodiacSign.Taurus] = language == "zh-CN" ? "感情生活有新发展，事业稳步前进。" : "Love life sees new developments, career progresses steadily.",
            // Add more monthly horoscopes...
        };
    }

    private Dictionary<string, double> GetCompatibilityMatrix()
    {
        return new Dictionary<string, double>
        {
            // Fire signs compatibility
            ["Aries_Leo"] = 0.9, ["Aries_Sagittarius"] = 0.85, ["Leo_Sagittarius"] = 0.88,
            ["Aries_Gemini"] = 0.8, ["Aries_Aquarius"] = 0.75, ["Leo_Gemini"] = 0.82,
            ["Leo_Libra"] = 0.84, ["Sagittarius_Aquarius"] = 0.86, ["Sagittarius_Libra"] = 0.78,
            
            // Earth signs compatibility
            ["Taurus_Virgo"] = 0.92, ["Taurus_Capricorn"] = 0.89, ["Virgo_Capricorn"] = 0.91,
            ["Taurus_Cancer"] = 0.86, ["Taurus_Pisces"] = 0.83, ["Virgo_Scorpio"] = 0.75,
            ["Capricorn_Scorpio"] = 0.81, ["Capricorn_Pisces"] = 0.77,
            
            // Air signs compatibility
            ["Gemini_Libra"] = 0.9, ["Gemini_Aquarius"] = 0.87, ["Libra_Aquarius"] = 0.89,
            
            // Water signs compatibility
            ["Cancer_Scorpio"] = 0.93, ["Cancer_Pisces"] = 0.91, ["Scorpio_Pisces"] = 0.88,
            
            // Cross-element compatible pairs
            ["Aries_Gemini"] = 0.8, ["Taurus_Cancer"] = 0.86, ["Leo_Libra"] = 0.84,
            ["Virgo_Scorpio"] = 0.75, ["Sagittarius_Aquarius"] = 0.86, ["Capricorn_Pisces"] = 0.77,
            
            // Challenging but workable pairs
            ["Aries_Cancer"] = 0.6, ["Aries_Capricorn"] = 0.65, ["Taurus_Leo"] = 0.63,
            ["Gemini_Virgo"] = 0.68, ["Cancer_Libra"] = 0.61, ["Leo_Scorpio"] = 0.64,
            ["Virgo_Sagittarius"] = 0.59, ["Libra_Capricorn"] = 0.62, ["Scorpio_Aquarius"] = 0.58,
            ["Sagittarius_Pisces"] = 0.66, ["Capricorn_Aries"] = 0.65, ["Aquarius_Taurus"] = 0.57,
            
            // Opposite signs (can be magnetic or conflicting)
            ["Aries_Libra"] = 0.72, ["Taurus_Scorpio"] = 0.69, ["Gemini_Sagittarius"] = 0.74,
            ["Cancer_Capricorn"] = 0.67, ["Leo_Aquarius"] = 0.71, ["Virgo_Pisces"] = 0.73
        };
    }
}
