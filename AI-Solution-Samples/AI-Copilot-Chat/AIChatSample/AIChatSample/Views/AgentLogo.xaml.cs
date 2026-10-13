using AIChatSample.Models;
using AIChatSample.ViewModel;
using Syncfusion.Maui.ListView;

namespace AIChatSample.Views.AIChat
{
    public partial class AgentLogoPage : ContentPage
    {
        private readonly AgentLogoPopupViewModel _vm;

        public AgentLogoPage()
        {
            InitializeComponent();
            _vm = new AgentLogoPopupViewModel();
            BindingContext = _vm;
            _vm.RequestClose += OnRequestClose;
            _vm.ResetForOpen();
            // The visible CollectionViews inside the popup body already keep
            // SelectedIcon / SelectedColor in sync via TwoWay binding. The
            // hidden addressable list views below are kept in lock-step so
            // external code (tests, future triggers) can read selection state
            // from a stable namescope.
            _vm.PropertyChanged += OnVmPropertyChanged;
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