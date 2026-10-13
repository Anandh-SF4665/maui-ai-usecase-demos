using Syncfusion.Maui.ListView;
using AIChatSample.Models;
using AIChatSample.ViewModel;

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
        }

        private void OnRequestClose(object? sender, EventArgs e)
        {
            if (Navigation?.ModalStack.Contains(this) == true)
            {
                Navigation.PopModalAsync(animated: true);
            }
        }

        private void IconsList_SelectionChanged(object? sender, ItemSelectionChangedEventArgs e)
        {
            if (IconsList.SelectedItem is LogoIconModel icon && _vm.SelectIconCommand.CanExecute(icon))
                _vm.SelectIconCommand.Execute(icon);
        }

        private void ColorsList_SelectionChanged(object? sender, ItemSelectionChangedEventArgs e)
        {
            if (ColorsList.SelectedItem is LogoColorModel color && _vm.SelectColorCommand.CanExecute(color))
                _vm.SelectColorCommand.Execute(color);
        }
    }
}