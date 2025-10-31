using System.Globalization;
using Microsoft.Maui.Graphics;

namespace KnewFate.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Green;
    public Color FalseColor { get; set; } = Colors.Transparent;

    // Accept nullable inputs per MAUI IValueConverter contract to avoid nullability warnings
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue ? (object)(boolValue ? TrueColor : FalseColor) : FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DoubleToPercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double doubleValue ? $"{doubleValue:P0}" : "0%";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnergyLevelToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue switch
            {
                >= 0.8 => Colors.Green,
                >= 0.6 => Colors.Orange,
                >= 0.4 => Colors.Yellow,
                _ => Colors.Red
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnergyLevelToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue switch
            {
                >= 0.9 => "Excellent",
                >= 0.8 => "Very Good",
                >= 0.7 => "Good",
                >= 0.6 => "Moderate",
                >= 0.5 => "Fair",
                >= 0.4 => "Low",
                _ => "Very Low"
            };
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ChartTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.ChartType chartType)
        {
            return chartType switch
            {
                Models.ChartType.Bazi => "bazi_icon.png",
                Models.ChartType.Astrology => "astrology_icon.png",
                Models.ChartType.Ziwei => "ziwei_icon.png",
                Models.ChartType.Numerology => "numerology_icon.png",
                Models.ChartType.Tarot => "tarot_icon.png",
                Models.ChartType.HumanDesign => "humandesign_icon.png",
                Models.ChartType.Fusion => "fusion_icon.png",
                _ => "default_chart.png"
            };
        }
        return "default_chart.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TarotSuitToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string suit)
        {
            return suit switch
            {
                "Major Arcana" => Color.FromArgb("#8B0000"),
                "Wands" => Color.FromArgb("#FF4500"),
                "Cups" => Color.FromArgb("#4169E1"),
                "Swords" => Color.FromArgb("#FFD700"),
                "Pentacles" => Color.FromArgb("#228B22"),
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class FiveElementToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string element)
        {
            return element switch
            {
                "木" or "Wood" => Color.FromArgb("#228B22"),
                "火" or "Fire" => Color.FromArgb("#DC143C"),
                "土" or "Earth" => Color.FromArgb("#DEB887"),
                "金" or "Metal" => Color.FromArgb("#C0C0C0"),
                "水" or "Water" => Color.FromArgb("#4682B4"),
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ZodiacSignToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sign)
        {
            return sign.ToLower() switch
            {
                "aries" => "♈",
                "taurus" => "♉",
                "gemini" => "♊",
                "cancer" => "♋",
                "leo" => "♌",
                "virgo" => "♍",
                "libra" => "♎",
                "scorpio" => "♏",
                "sagittarius" => "♐",
                "capricorn" => "♑",
                "aquarius" => "♒",
                "pisces" => "♓",
                _ => "?"
            };
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CompatibilityScoreToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double score)
        {
            return score switch
            {
                >= 0.8 => Color.FromArgb("#32CD32"), // Lime Green
                >= 0.6 => Color.FromArgb("#FFD700"), // Gold
                >= 0.4 => Color.FromArgb("#FFA500"), // Orange
                _ => Color.FromArgb("#FF6347") // Tomato
            };
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateTimeToRelativeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalDays < 1)
            {
                if (timeSpan.TotalHours < 1)
                {
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                }
                return $"{(int)timeSpan.TotalHours} hours ago";
            }

            if (timeSpan.TotalDays < 7)
            {
                return $"{(int)timeSpan.TotalDays} days ago";
            }

            if (timeSpan.TotalDays < 30)
            {
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            }

            if (timeSpan.TotalDays < 365)
            {
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            }

            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanInverterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool boolValue ? !boolValue : false;
    }
}

public class StringEmptyToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ListCountToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is System.Collections.ICollection collection)
        {
            return collection.Count > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PercentToProgressConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            // Assumes value is 0-100, convert to 0-1 for ProgressBar
            return doubleValue / 100.0;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
