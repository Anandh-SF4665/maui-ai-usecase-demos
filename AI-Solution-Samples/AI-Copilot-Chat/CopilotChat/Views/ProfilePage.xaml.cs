using CopilotChat.Services;

namespace CopilotChat.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ChatDataService dataService;

    public ProfilePage(ChatDataService dataService)
    {
        InitializeComponent();
        this.dataService = dataService;

        NameEntry.Text = dataService.Profile.Name;
        EmailEntry.Text = dataService.Profile.Email;
        InitialLabel.Text = dataService.Profile.Name.Length > 0
            ? dataService.Profile.Name.Substring(0, 1).ToUpperInvariant()
            : "?";
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        dataService.Profile.Name = NameEntry.Text?.Trim() ?? dataService.Profile.Name;
        dataService.Profile.Email = EmailEntry.Text?.Trim() ?? dataService.Profile.Email;

        await DisplayAlert("Profile", "Profile updated successfully.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
