using AIChatSample.Controls;
using AIChatSample.Models;
using AIChatSample.Services;
using AIChatSample.ViewModel;

namespace AIChatSample.Views.AIChat;

public partial class SearchPage : ContentPage
{
	public SearchPage()
	{
		InitializeComponent();
            // Bind to the shared app shell view-model so Search, Library
            // and Recent chats reflect the same in-memory state (FR-0.1,
            // FR-4.1).
            this.BindingContext = ServiceHelper.GetService<AIMainLayoutViewModel>() ?? new AIMainLayoutViewModel();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatToolbar.BackRequested -= OnBackRequested;
        }
        else
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;
            AIChatToolbar.BackRequested -= OnBackRequested;
            AIChatToolbar.BackRequested += OnBackRequested;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AIChatToolbar.IsBackVisible = Navigation?.NavigationStack.Count > 1;
    }

    private async void OnBackRequested(object? sender, EventArgs e)
    {
        if (Navigation?.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestEventArgs e)
    {
        ChatNavigationService.Handle(this, e.Key, e.Parameter);
    }

    private void OnNavigationMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }

    /// <summary>
    /// Opens the tapped recent chat by routing through the chat
    /// navigation service (FR-4.1 + FR-0.4). Equivalent to tapping the
    /// same row in the left-nav drawer.
    /// </summary>
    private void RecentChatItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is RecentChatItem item)
        {
            ChatNavigationService.Handle(this, "OpenChat", item);
        }
    }
}