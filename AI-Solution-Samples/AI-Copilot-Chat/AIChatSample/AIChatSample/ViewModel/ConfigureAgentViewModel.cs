using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.Chat;

namespace AIChatSample.ViewModel;

public partial class ConfigureAgentViewModel : ObservableObject
{
    public ConfigureAgentViewModel()
    {
        CurrentUser = new Author { Name = "You" };
        Messages = new ObservableCollection<object>();

        // Initial assistant prompt shown above the chat
        Messages.Add(new TextMessage
        {
            Author = new Author { Name = "Agent" },
            Text = "Hi! Tell me what kind of specialist agent you'd like me to build.",
        });
    }

    [ObservableProperty] private string messageText = string.Empty;

    [ObservableProperty] private Author? currentUser;

    [ObservableProperty] private ObservableCollection<object> messages;


    [RelayCommand] private void Configure() { }

    [RelayCommand] private void CreateAgent() { }

    [RelayCommand] private void AddAttachment() { }

    [RelayCommand] private void Voice() { }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        Messages.Add(new TextMessage
        {
            Author = CurrentUser,
            Text = MessageText.Trim(),
        });
        MessageText = string.Empty;

        // Mock reply
        await Task.Delay(300);
        Messages.Add(new TextMessage
        {
            Author = new Author { Name = "Agent" },
            Text = "Got it — I'll fold that into the agent spec.",
        });
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(MessageText);
}