using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace AIChatSample.Controls;

public partial class AIChatToolbar : ContentView
{
    public static readonly BindableProperty ModelOptionsProperty =
        BindableProperty.Create(
            nameof(ModelOptions),
            typeof(IList<string>),
            typeof(AIChatToolbar),
            new ObservableCollection<string> { "Model 1", "Model 2", "Model 3" });

    public static readonly BindableProperty SelectedModelProperty =
        BindableProperty.Create(nameof(SelectedModel), typeof(string), typeof(AIChatToolbar), "Model 1");

    public static readonly BindableProperty MessageCommandProperty =
        BindableProperty.Create(nameof(MessageCommand), typeof(ICommand), typeof(AIChatToolbar));

    public static readonly BindableProperty IsModelSelectorVisibleProperty =
        BindableProperty.Create(nameof(IsModelSelectorVisible), typeof(bool), typeof(AIChatToolbar), true);

    public static readonly BindableProperty IsNavigationMenuVisibleProperty =
        BindableProperty.Create(nameof(IsNavigationMenuVisible), typeof(bool), typeof(AIChatToolbar), true);

    public static readonly BindableProperty IsBackVisibleProperty =
        BindableProperty.Create(nameof(IsBackVisible), typeof(bool), typeof(AIChatToolbar), false);

    public AIChatToolbar()
    {
        InitializeComponent();
    }

    public IList<string> ModelOptions
    {
        get => (IList<string>)GetValue(ModelOptionsProperty);
        set => SetValue(ModelOptionsProperty, value);
    }

    public string SelectedModel
    {
        get => (string)GetValue(SelectedModelProperty);
        set => SetValue(SelectedModelProperty, value);
    }

    public ICommand? MessageCommand
    {
        get => (ICommand?)GetValue(MessageCommandProperty);
        set => SetValue(MessageCommandProperty, value);
    }

    public bool IsModelSelectorVisible
    {
        get => (bool)GetValue(IsModelSelectorVisibleProperty);
        set => SetValue(IsModelSelectorVisibleProperty, value);
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
