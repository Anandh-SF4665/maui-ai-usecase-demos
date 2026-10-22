using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class SettingsPage : ContentView
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is SettingsViewModel vm)
        {
            vm.OnDeleteVehicleConfirmationRequested = async vehicle =>
            {
                var page = Application.Current?.MainPage;
                return page != null && await page.DisplayAlert(
                    "Delete vehicle",
                    $"Delete {vehicle.MakeModel} and all of its service, fuel, and schedule data?",
                    "Delete",
                    "Cancel");
            };

            // The view owns navigation — route the sign-out request through Shell.
            vm.OnNavigateToWelcomeRequested -= OnNavigateToWelcome;
            vm.OnNavigateToWelcomeRequested += OnNavigateToWelcome;
        }
    }

    private async void OnNavigateToWelcome()
    {
        try { await Shell.Current.GoToAsync("///welcome"); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
        }
    }
}
