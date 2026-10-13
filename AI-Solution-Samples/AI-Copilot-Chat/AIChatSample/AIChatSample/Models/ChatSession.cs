using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample.Models;

/// <summary>
/// Single conversation between the user and the AI. Holds the message
/// log plus the agent binding so the chat can be re-opened with the same
/// system context (FR-0.4).
/// </summary>
public sealed class ChatSession : INotifyPropertyChanged
{
    private string title = string.Empty;
    private DateTime lastActiveDate = DateTime.UtcNow;

    public ChatSession()
    {
    }

    public ChatSession(string id, string title, AgentConfiguration? agent = null)
    {
        Id = id;
        Title = title;
        Agent = agent;
    }

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Title
    {
        get => title;
        set
        {
            if (title == value) return;
            title = value;
            OnPropertyChanged();
        }
    }

    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public DateTime LastActiveDate
    {
        get => lastActiveDate;
        set
        {
            if (lastActiveDate == value) return;
            lastActiveDate = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Agent the session is bound to, or <c>null</c> for the default Copilot.</summary>
    public AgentConfiguration? Agent { get; set; }

    public ObservableCollection<IAssistItem> Messages { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
