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
            _vm.AvailableIcons.CollectionChanged += (_, __) => SyncIconItemsSource();
            _vm.AvailableColors.CollectionChanged += (_, __) => SyncColorItemsSource();
            SyncIconItemsSource();
            SyncColorItemsSource();
            _vm.ResetForOpen();
        }

        private void OnRequestClose(object? sender, EventArgs e)
        {
            if (Navigation?.ModalStack.Contains(this) == true)
            {
                Navigation.PopModalAsync(animated: true);
            }
        }

        private void SyncIconItemsSource()
        {
            IconsList.ItemsSource = _vm.AvailableIcons;
        }

        private void SyncColorItemsSource()
        {
            ColorsList.ItemsSource = _vm.AvailableColors;
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

        // The visible mirror CollectionViews live inside the popup's
        // DataTemplate, so they cannot be reached via x:Name. We instead
        // receive their SelectionChanged through routed events on the
        // popup's host element. Because the mirrors bind to the same
        // collections, we just forward the change into the VM via the
        // same selection logic.
        private void IconsMirror_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is LogoIconModel icon && _vm.SelectIconCommand.CanExecute(icon))
                _vm.SelectIconCommand.Execute(icon);
        }

        private void ColorsMirror_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is LogoColorModel color && _vm.SelectColorCommand.CanExecute(color))
                _vm.SelectColorCommand.Execute(color);
        }
    }
}
