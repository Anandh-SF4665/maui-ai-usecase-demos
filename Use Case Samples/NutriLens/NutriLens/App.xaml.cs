using NutriLens.Views;

namespace NutriLens
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Splash screen delays 3s then pushes the real Dashboard.
            // AppNavigator expects the root to be a NavigationPage.
            return new Window(new NavigationPage(new MainPage()));
        }
    }
}