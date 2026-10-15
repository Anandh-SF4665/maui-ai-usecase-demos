using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace AIChatSample.Controls;

public partial class AIChatToolbar : ContentView
{
    public static readonly BindableProperty MessageCommandProperty =
        BindableProperty.Create(nameof(MessageCommand), typeof(ICommand), typeof(AIChatToolbar));

    public static readonly BindableProperty IsNavigationMenuVisibleProperty =
        BindableProperty.Create(nameof(IsNavigationMenuVisible), typeof(bool), typeof(AIChatToolbar), true);

    public static readonly BindableProperty IsBackVisibleProperty =
        BindableProperty.Create(nameof(IsBackVisible), typeof(bool), typeof(AIChatToolbar), false);

    public AIChatToolbar()
    {
        InitializeComponent();
    }

    public ICommand? MessageCommand
    {
        get => (ICommand?)GetValue(MessageCommandProperty);
        set => SetValue(MessageCommandProperty, value);
    }

    public bool IsNavigationMenuVisible
    {
        get => (bool)GetValue(IsNavigationMenuVisibleProperty);
        set => SetValue(IsNavigationMenuVisibleProperty, value);
    }

    public bool IsBackVisible
    {
        get => (bool)GetValue(IsBackVisibleProperty);
        set => SetValue(IsBackVisibleProperty, value);
    }

    public event EventHandler? NavigationMenuTapped;

    public event EventHandler? BackRequested;

    private void OnNavigationMenuTapped(object sender, TappedEventArgs e)
    {
        NavigationMenuTapped?.Invoke(this, EventArgs.Empty);
    }

    private void OnBackTapped(object? sender, TappedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
