using System.Collections.ObjectModel;
using AIChatSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIChatSample.ViewModel
{

    public partial class ProfileSettingsViewModel : ObservableObject
    {
        public ProfileSettingsViewModel()
        {
            // --- Profile header ---
            DisplayName     = "Alexa Developer";
            EmailAddress    = "alex.dev@example.com";
            AccountType     = "Google Account";
            AvatarSource    = "userprofile.png";

            // --- Plan ---
            PlanName        = "Enterprise Pro";
            BillingCycle    = "Billed annually";
            PlanFeatures    = new ObservableCollection<string>
            {
                "Unlimited AI Chats",
                "Priority Processing",
            };

            // --- Connected services ---
            ConnectedServices = new ObservableCollection<ConnectedService>
            {
                new() { Name = "Google Drive", IconSource = "googledrive.png",  IsConnected = true,  ActionText = "" },
                new() { Name = "OneDrive",     IconSource = "onedrive.png",     IsConnected = false, ActionText = "Connect" },
            };
        }
         
        [ObservableProperty] private string? displayName;
        [ObservableProperty] private string? emailAddress;
        [ObservableProperty] private string? accountType;
        [ObservableProperty] private string? avatarSource;
         
        [ObservableProperty] private string? planName;
        [ObservableProperty] private string? billingCycle;
        public ObservableCollection<string> PlanFeatures { get; }
         
        public ObservableCollection<ConnectedService> ConnectedServices { get; }
           
        public enum LayoutMode { Mobile, Desktop }
        [ObservableProperty] private bool isDrawerOpen; 
         
        [RelayCommand] private void EditProfile()        {  }
        [RelayCommand] private void ManageBilling()      {  }
        [RelayCommand] private void DisconnectService(ConnectedService svc)
        {
            // Toggle connection off in place
            var idx = ConnectedServices.IndexOf(svc);
            if (idx >= 0)
            {
                ConnectedServices[idx] = new ConnectedService
                {
                    Name        = svc.Name,
                    IconSource  = svc.IconSource,
                    IsConnected = false,
                    ActionText  = "Connect",
                };
            }
        }
        [RelayCommand] private void ConnectService(ConnectedService svc)
        {
            var idx = ConnectedServices.IndexOf(svc);
            if (idx >= 0)
            {
                ConnectedServices[idx] = new ConnectedService
                {
                    Name        = svc.Name,
                    IconSource  = svc.IconSource,
                    IsConnected = true,
                    ActionText  = "",
                };
            }
        }
        [RelayCommand] private async Task RequestExportAsync()
        {
            await Task.CompletedTask;  
        }
        [RelayCommand] private async Task SignOutAsync()
        {
            await Task.CompletedTask;  
        }
        [RelayCommand] private async Task DeleteAccountAsync()
        {
            await Task.CompletedTask;  
        }
         
    }
}