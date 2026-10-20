namespace AIChatSample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Register the Syncfusion license before any Syncfusion control is
        // inflated. Replace the placeholder with a real key (or pull it
        // from a secure store) to silence the runtime license banner.
        // TODO: Replace with your Syncfusion license key, or load it from
        //       a secure store (e.g. environment variable, user-secrets).
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Initial route: NewChat wrapped in a NavigationPage so Profile,
        // Library, CreateAgent, etc. can be pushed onto the stack.
        return new Window(new AppShell());
    }
}