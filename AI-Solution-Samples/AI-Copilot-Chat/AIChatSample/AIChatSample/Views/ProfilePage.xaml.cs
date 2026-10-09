using AIChatSample.Controls;
using AIChatSample.Services;
using AIChatSample.ViewModel;

namespace AIChatSample.Views.AIChat
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            this.BindingContext = ServiceHelper.GetService<AIMainLayoutViewModel>() ?? new AIMainLayoutViewModel();
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

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

        private void OnNavigationMenuTapped(object sender, EventArgs e)
        {
#if ANDROID
            AIChatNavigationPanel.ToggleCompact();
#endif
        }
    }
}