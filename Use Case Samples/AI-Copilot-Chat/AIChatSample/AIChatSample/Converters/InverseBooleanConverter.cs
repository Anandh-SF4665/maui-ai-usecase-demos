using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Inverts a boolean value. Returns <c>true</c> when the source value is <c>false</c>,
/// and <c>false</c> when the source value is <c>true</c>. Non-boolean values are treated
/// as <c>false</c> on input.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : false;
    }
}
