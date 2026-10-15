using System.Globalization;

// Aliases for the Syncfusion toggle-glyph converter below.
// The header template's binding context is a GroupResult, so we
// receive that type at runtime even though XAML can't see it.
using GResult = Syncfusion.Maui.DataSource.Extensions.GroupResult;

namespace ListView_IntentGrouping.Converters;

/// <summary>
/// Returns <c>True</c> when the integer is greater than zero.
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns a 5-character rating string (e.g. "\u2605\u2605\u2605\u2605\u2606")
/// based on an integer rating from 1 to 5.
/// </summary>
public class RatingToStarsConverter : IValueConverter
{
    private const string Filled = "\u2605";
    private const string Empty = "\u2606";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int rating = value is int i ? Math.Clamp(i, 0, 5) : 0;
        return string.Concat(Enumerable.Repeat(Filled, rating))
             + string.Concat(Enumerable.Repeat(Empty, 5 - rating));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps an integer priority (1-5) to a friendly label:
///   1 = Low, 2-3 = Medium, 4-5 = High.
/// </summary>
public class PriorityToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int priority = value is int i ? i : 0;
        return priority switch
        {
            >= 4 => "High",
            >= 2 => "Medium",
            >= 1 => "Low",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps an integer priority (1-5) to a background colour that
/// matches the severity. Used to colour-code the priority chip on
/// each item card.
/// </summary>
public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int priority = value is int i ? i : 0;
        return priority switch
        {
            >= 4 => "#A50E0E", // red - high
            >= 2 => "#B45F06", // amber - medium
            >= 1 => "#5F6368", // grey - low
            _ => "#5F6368"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
/// <summary>
/// Maps the current <see cref="GResult.IsExpand"/> value to a
/// chevron glyph that visually matches the group's expand state.
///
/// * Expanded  -> down-pointing chevron (rotate 0°) so the user
///                sees "this group is open, click to collapse".
/// * Collapsed -> right-pointing chevron (rotated 90°) so the user
///                sees "this group is closed, click to expand".
///
/// We use Unicode chevrons (\uE5C5 / \uE5C8) rendered with the
/// bundled OpenSans font — keeps the existing "MaterialAssets"
/// glyph look without depending on a font file that isn't
/// registered in the .csproj.
/// </summary>
public sealed class GroupExpandGlyphConverter : IValueConverter
{
    // \uE5C5 = keyboard arrow down (open)
    // \uE5C8 = keyboard arrow right (closed)
    private const string DownGlyph  = "\ue705";
    private const string RightGlyph = "\ue706";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Defensive: Syncfusion may push a non-bool here while the
        // groups are being re-projected (the binding context is
        // briefly null). Fall back to the "closed" glyph, which is
        // the visually safer default.
        bool isExpanded = value is bool b && b;
        return isExpanded ? DownGlyph : RightGlyph;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps <see cref="GResult.IsExpand"/> to a rotation in degrees so
/// a single right-pointing chevron animates into a down-pointing
/// chevron when the group is expanded. Pairs with
/// <see cref="GroupExpandGlyphConverter"/> when the designer
/// prefers a single, rotating glyph over two separate ones.
/// </summary>
public sealed class GroupExpandRotationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isExpanded = value is bool b && b;
        return isExpanded ? 0d : -90d;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}