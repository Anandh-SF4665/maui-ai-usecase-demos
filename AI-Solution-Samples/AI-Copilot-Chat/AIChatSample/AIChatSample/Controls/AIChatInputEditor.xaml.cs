using System.ComponentModel;
using System.Windows.Input;

namespace AIChatSample.Controls;

public partial class AIChatInputEditor : ContentView
{
    // Text
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(AIChatInputEditor),
            string.Empty,
            BindingMode.TwoWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // Placeholder
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(AIChatInputEditor),
            "Type Message...");

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // Attachement icon
    public static readonly BindableProperty AddIconProperty =
        BindableProperty.Create(
            nameof(AddIcon),
            typeof(ImageSource),
            typeof(AIChatInputEditor),
            ImageSource.FromFile("plus_ucon.png"));

    public ImageSource AddIcon
    {
        get => (ImageSource)GetValue(AddIconProperty);
        set => SetValue(AddIconProperty, value);
    }

    // Mic icon
    public static readonly BindableProperty MicIconProperty =
        BindableProperty.Create(
            nameof(MicIcon),
            typeof(ImageSource),
            typeof(AIChatInputEditor),
            ImageSource.FromFile("voice_icon.png"));

    public ImageSource MicIcon
    {
        get => (ImageSource)GetValue(MicIconProperty);
        set => SetValue(MicIconProperty, value);
    }

    // Send icon
    public static readonly BindableProperty SendIconProperty =
        BindableProperty.Create(
            nameof(SendIcon),
            typeof(ImageSource),
            typeof(AIChatInputEditor),
            ImageSource.FromFile("send_button_icon.png"));

    public ImageSource SendIcon
    {
        get => (ImageSource)GetValue(SendIconProperty);
        set => SetValue(SendIconProperty, value);
    }

    // Send command
    public static readonly BindableProperty SendCommandProperty =
        BindableProperty.Create(
            nameof(SendCommand),
            typeof(ICommand),
            typeof(AIChatInputEditor),
            default(ICommand));

    public ICommand SendCommand
    {
        get => (ICommand)GetValue(SendCommandProperty);
        set => SetValue(SendCommandProperty, value);
    }

    public AIChatInputEditor()
	{
		InitializeComponent();
	}
    private void Icon_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#3A393B")
                : Color.FromArgb("#F2F2F2");

            border.Scale = 1.05;
        }
    }

    private void Icon_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Colors.Transparent;
            border.Scale = 1.0;
        }
    }
    private Shadow? _hoverShadow;

    private void EditorBorder_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.15f,
                Radius = 12,
                Offset = new Point(0, 2)
            };
        }
    }
    private void InputEditor_Unfocused(object sender, FocusEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border != null)
        {
            border.Stroke = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#5A5A5A")
                : Color.FromArgb("#D1D1D1");

            border.StrokeThickness = 1;
            border.Shadow = null;
        }
    }
    private void InputEditor_Focused(object sender, FocusEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border != null)
        {
            border.Stroke = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#8A8A8A")
                : Color.FromArgb("#777777");

            border.StrokeThickness = 1.5;

            border.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.18f,
                Radius = 12,
                Offset = new Point(0, 2)
            };
        }
    }
    private void EditorBorder_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            if (border.StrokeThickness > 1)
                return;

            border.Shadow = null;
        }
    }
    private Border? GetParentBorder(Element element)
    {
        var parent = element.Parent;

        while (parent != null)
        {
            if (parent is Border border)
                return border;

            parent = parent.Parent;
        }

        return null;
    }
    private void InputEditor_PropertyChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not VisualElement editor)
            return;

        var border = GetParentBorder(editor);

        if (border == null)
            return;

        bool hasText = !string.IsNullOrWhiteSpace(e.NewTextValue);

        if (hasText)
        {
            border.Stroke = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#8A8A8A")
                : Color.FromArgb("#7A7A7A");

            border.StrokeThickness = 1;
        }
        else
        {
            border.Stroke = Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#5A5A5A")
                : Color.FromArgb("#D1D1D1");

            border.StrokeThickness = 1;
        } 
    }
}