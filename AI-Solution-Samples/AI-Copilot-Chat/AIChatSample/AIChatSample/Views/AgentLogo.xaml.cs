using AIChatSample.Models;
using AIChatSample.ViewModel;
using Syncfusion.Maui.ListView;

namespace AIChatSample.Views.AIChat
{
    public partial class AgentLogoPage : ContentPage
    {
        private AgentLogoPopupViewModel _vm;

        public AgentLogoPage() : this(null) { }

        public AgentLogoPage(AgentLogoPopupViewModel? viewModel)
        {
            InitializeComponent();
            _vm = viewModel ?? new AgentLogoPopupViewModel();
            if (BindingContext is null) BindingContext = _vm;
            _vm.RequestClose += OnRequestClose;
            _vm.ResetForOpen();
            // The visible CollectionViews inside the popup body keep
            // SelectedIcon / SelectedColor in sync via TwoWay binding. The
            // hidden addressable list views are kept in lock-step so
            // external code (tests, future triggers) can read selection state
            // from a stable namescope.
            _vm.PropertyChanged += OnVmPropertyChanged;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is AgentLogoPopupViewModel incoming && incoming != _vm)
            {
                _vm.RequestClose -= OnRequestClose;
                _vm.PropertyChanged -= OnVmPropertyChanged;
                _vm = incoming;
                _vm.RequestClose += OnRequestClose;
                _vm.PropertyChanged += OnVmPropertyChanged;
                _vm.ResetForOpen();
            }
        }

        private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AgentLogoPopupViewModel.SelectedIcon))
            {
                IconsList.SelectedItem = _vm.SelectedIcon;
            }
            else if (e.PropertyName == nameof(AgentLogoPopupViewModel.SelectedColor))
            {
                ColorsList.SelectedItem = _vm.SelectedColor;
            }
        }

        private void OnRequestClose(object? sender, EventArgs e)
        {
            LogoPopup.IsOpen = false;
            if (Navigation?.ModalStack.Contains(this) == true)
            {
                Navigation.PopModalAsync(animated: true);
            }
        }

        private void IconsList_SelectionChanged(object? sender, ItemSelectionChangedEventArgs e)
        {
            if (sender is SfListView list && list.SelectedItem is LogoIconModel icon &&
                _vm.SelectIconCommand.CanExecute(icon))
            {
                _vm.SelectIconCommand.Execute(icon);
            }
        }

        private void ColorsList_SelectionChanged(object? sender, ItemSelectionChangedEventArgs e)
        {
            if (sender is SfListView list && list.SelectedItem is LogoColorModel color &&
                _vm.SelectColorCommand.CanExecute(color))
            {
                _vm.SelectColorCommand.Execute(color);
            }
        }
    }
}