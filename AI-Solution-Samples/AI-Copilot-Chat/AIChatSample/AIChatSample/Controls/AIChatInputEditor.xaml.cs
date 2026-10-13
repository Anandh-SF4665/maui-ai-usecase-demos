using System.ComponentModel;
using System.Windows.Input;

namespace AIChatSample.Controls;

public partial class AIChatInputEditor : ContentView
{
    private const string SendEnabledColorLight = "#7633DA";
    private const string SendEnabledColorDark = "#A572F7";
    private const string SendDisabledColorLight = "#C9C6C8";
    private const string SendDisabledColorDark = "#858385";

    // Text
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(AIChatInputEditor),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextPropertyChanged);

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

    // IsSendEnabled (read-only) - true when text is non-empty AND command can execute
    public static readonly BindableProperty IsSendEnabledProperty =
        BindableProperty.Create(
            nameof(IsSendEnabled),
            typeof(bool),
            typeof(AIChatInputEditor),
            false);

    public bool IsSendEnabled
    {
        get => (bool)GetValue(IsSendEnabledProperty);
        private set => SetValue(IsSendEnabledProperty, value);
    }

    public AIChatInputEditor()
    {
        InitializeComponent();
        UpdateSendVisualState();
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AIChatInputEditor editor)
        {
            editor.UpdateSendVisualState();
        }
    }

    private void UpdateSendVisualState()
    {
        bool hasText = !string.IsNullOrWhiteSpace(this.Text);
        bool canExecute = this.SendCommand?.CanExecute(this.Text) ?? false;
        bool isEnabled = hasText && canExecute;

        if (this.IsSendEnabled != isEnabled)
        {
            this.IsSendEnabled = isEnabled;
        }

        // Visually grey out / activate the icon.
        if (this.SendIconLabel is not null)
        {
            bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            this.SendIconLabel.TextColor = isEnabled
                ? Color.FromArgb(isDark ? SendEnabledColorDark : SendEnabledColorLight)
                : Color.FromArgb(isDark ? SendDisabledColorDark : SendDisabledColorLight);
        }
    }

    private void OnSendTapped(object? sender, TappedEventArgs e)
    {
        if (!this.IsSendEnabled)
        {
            return;
        }

        var command = this.SendCommand;
        if (command is null)
        {
            return;
        }

        if (command.CanExecute(this.Text))
        {
            command.Execute(this.Text);
        }
    }

    private void SendIcon_PointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border && this.IsSendEnabled)
        {
            border.BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#3A393B")
                : Color.FromArgb("#F2F2F2");

            border.Scale = 1.05;
        }
    }

    private void SendIcon_PointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            border.BackgroundColor = Colors.Transparent;
            border.Scale = 1.0;
        }
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

        // Keep the send button state in sync with the latest text.
        this.UpdateSendVisualState();
    }
}
