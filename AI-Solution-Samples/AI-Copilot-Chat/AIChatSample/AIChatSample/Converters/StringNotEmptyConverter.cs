using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Returns <c>true</c> when the bound string is non-null and non-empty.
/// Used to drive <c>IsVisible</c> bindings on agent avatar fallbacks:
/// <c>IsVisible="{Binding AvatarSource, Converter={StaticResource StringNotEmpty}}"</c>
/// hides the initial-letter fallback when an image source is supplied.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
