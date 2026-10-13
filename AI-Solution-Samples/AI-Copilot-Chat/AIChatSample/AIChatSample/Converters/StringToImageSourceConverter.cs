using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Converts a file path or URL string into an <see cref="ImageSource"/> so an
/// <c>Image.Source</c> binding can render an image selected at runtime (e.g.
/// via <c>FilePicker</c>). Returns <c>null</c> for null/whitespace input.
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        if (Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return ImageSource.FromUri(uri);
        }

        return ImageSource.FromFile(s);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingValue.UnsetValue;
    }
}
