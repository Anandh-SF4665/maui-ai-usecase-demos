using System;
using Microsoft.Maui.ApplicationModel;
using System.ComponentModel;

namespace SmartDailyDecisionCompanion
{
    public partial class MainPage : ContentPage
    {
        private bool _isNavigatingToInsights;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(MainViewModel.SelectedTask))
                return;

            var vm = BindingContext as MainViewModel;
            if (vm?.SelectedTask == null || _isNavigatingToInsights)
                return;

            _isNavigatingToInsights = true;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (MobileTabView != null && MobileTabView.IsVisible)
                        MobileTabView.SelectedIndex = 1;
                    if (DesktopTabView != null && DesktopTabView.IsVisible)
                        DesktopTabView.SelectedIndex = 1;
                }
                finally
                {
                    _isNavigatingToInsights = false;
                }
            });
        }

        private void OnAcceptRecommendationClicked(object? sender, EventArgs e)
        {
            NavigateToTodayTab();
        }

        private void OnPostponeRecommendationClicked(object? sender, EventArgs e)
        {
            NavigateToTodayTab();
        }

        private void NavigateToTodayTab()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (MobileTabView != null && MobileTabView.IsVisible)
                    MobileTabView.SelectedIndex = 0;
                if (DesktopTabView != null && DesktopTabView.IsVisible)
                    DesktopTabView.SelectedIndex = 0;
            });
        }
    }
}
