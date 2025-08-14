namespace KnewFate.Models;

/// <summary>
/// 西方十二星座
/// </summary>
public enum ZodiacSign
{
    Aries,      // 白羊座
    Taurus,     // 金牛座
    Gemini,     // 双子座
    Cancer,     // 巨蟹座
    Leo,        // 狮子座
    Virgo,      // 处女座
    Libra,      // 天秤座
    Scorpio,    // 天蝎座
    Sagittarius,// 射手座
    Capricorn,  // 摩羯座
    Aquarius,   // 水瓶座
    Pisces      // 双鱼座
}

/// <summary>
/// 中国十二生肖
/// </summary>
public enum ChineseZodiac
{
    Rat,    // 鼠
    Ox,     // 牛
    Tiger,  // 虎
    Rabbit, // 兔
    Dragon, // 龙
    Snake,  // 蛇
    Horse,  // 马
    Goat,   // 羊
    Monkey, // 猴
    Rooster,// 鸡
    Dog,    // 狗
    Pig     // 猪
}

/// <summary>
/// 星座档案
/// </summary>
public class ZodiacProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ZodiacSign ZodiacSign { get; set; }
    public ChineseZodiac ChineseZodiac { get; set; }
    public DateTime BirthDate { get; set; }
    public TimeSpan BirthTime { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
