using System.Globalization;
using System.Windows.Input;

namespace CopilotChat.Views;

public partial class AgentLogoEditor : ContentView
{
    public AgentLogoEditor()
    {
        InitializeComponent();
    }
}

/// <summary>
/// Highlights a color swatch when it matches the view model's selected color.
/// Binding context of the swatch is the hex color string.
/// </summary>
public class ColorToHighlightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var vm = AgentLogoEditorVmHolder.Current;
        return value as string == vm?.SelectedColor ? Colors.Transparent : (object)Application.Current!.Resources["DividerColor"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

internal static class AgentLogoEditorVmHolder
{
    public static ViewModels.CreateAgentViewModel? Current { get; set; }
}
