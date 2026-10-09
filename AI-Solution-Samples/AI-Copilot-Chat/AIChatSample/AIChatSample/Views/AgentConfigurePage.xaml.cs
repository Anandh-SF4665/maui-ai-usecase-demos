using AIChatSample.Controls;
using AIChatSample.Services;
using AIChatSample.ViewModel;

namespace AIChatSample.Views.AIChat
{
    public partial class AgentConfigurePage : ContentPage
    {
        /// <summary>Backing view-model (constructor-injectable for tests).</summary>
        internal AgentConfigurePageViewModel Vm { get; }

        /// <summary>DI/fallback constructor — resolves shared services.</summary>
        public AgentConfigurePage()
            : this(new AgentConfigurePageViewModel(
                ServiceHelper.GetRequiredService<IAgentService>(),
                ServiceHelper.GetRequiredService<IAgentStore>()))
        {
        }

        /// <summary>Preferred constructor — used by DI and unit tests.</summary>
        internal AgentConfigurePage(AgentConfigurePageViewModel viewModel)
        {
            InitializeComponent();
            Vm = viewModel;
            // Composite VM: the XAML binds to a flat shape (AgentName,
            // Instructions, SelectedTab, ...) but the navigation panel
            // still needs the AIMainLayoutViewModel for sidebar state.
            BindingContext = Vm;
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

        private void AndroidMenuTapped(object? sender, EventArgs e)
        {
#if ANDROID
            AIChatNavigationPanel.ToggleCompact();
#endif
        }

        private void OnConfigureTabTapped(object? sender, TappedEventArgs e)
        {
            if (BindingContext is AgentConfigurePageViewModel vm)
            {
                vm.SelectedTab = 0;
            }
        }

        private void OnPreviewTabTapped(object? sender, TappedEventArgs e)
        {
            if (BindingContext is AgentConfigurePageViewModel vm)
            {
                vm.SelectedTab = 1;
            }
        }

        /// <summary>
        /// Promotes the inline name-required error as soon as the user
        /// gives the Name field focus (FR-1.2 / AC-1.2). Without this,
        /// the error would only surface after a failed Create — which
        /// is fine but later than the form-validation feedback the
        /// Figma flow expects.
        /// </summary>
        private void OnAgentNameFocused(object? sender, FocusEventArgs e)
        {
            if (BindingContext is AgentConfigurePageViewModel vm)
            {
                vm.NotifyNameInteracted();
            }
        }

        /// <summary>
        /// Re-evaluates the inline name-required error when the user
        /// leaves the field. If they tabbed through and left it empty,
        /// the error now appears; if they typed something the error
        /// stays hidden (FR-1.2 / AC-1.2).
        /// </summary>
        private void OnAgentNameUnfocused(object? sender, FocusEventArgs e)
        {
            if (BindingContext is AgentConfigurePageViewModel vm)
            {
                vm.NotifyNameInteracted();
            }
        }

        private async void OnCreateAgentClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not AgentConfigurePageViewModel vm || !vm.IsCreateEnabled)
            {
                return;
            }

            // Run the MVVM command so validation, busy state, and the store
            // are handled exactly as in unit tests.
            await vm.AgentVM.CreateAgentCommand.ExecuteAsync(null);

            if (!vm.CreatedSuccessfully)
            {
                // Surface inline error message when creation failed.
                await DisplayAlert("Can't create agent", vm.ErrorMessage ?? "Please try again.", "OK");
                return;
            }

            SemanticScreenReader.Announce($"Agent {vm.AgentVM.AgentName} created");

            // Surface a tiny confirmation then go back to the
            // create-agent page so the user can keep iterating.
            if (Application.Current?.MainPage is Page host)
            {
                await host.DisplayAlert("Agent created",
                    $"Specialist agent '{vm.AgentVM.AgentName}' is ready.", "OK");
            }

            if (Navigation?.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync(animated: true);
            }
        }
    }
}