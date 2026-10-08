using System.Collections.ObjectModel;
using CopilotChat.Models;

namespace CopilotChat.Services;

/// <summary>
/// In-memory store for agents, chat sessions and the user profile.
/// </summary>
public class ChatDataService
{
    public ObservableCollection<Agent> Agents { get; } = new();

    public ObservableCollection<ChatSession> ChatHistory { get; } = new();

    public UserProfile Profile { get; } = new();

    public ChatSession CurrentChat { get; set; }

    /// <summary>Library entries (pinned conversations / generated content).</summary>
    public ObservableCollection<LibraryItem> LibraryItems { get; } = new();

    public ChatDataService()
    {
        Agents.Add(new Agent
        {
            Name = "Fitness Coach",
            Description = "Personal trainer that builds workout plans and tracks progress.",
            Behaviour = "You are an encouraging fitness coach. Give practical, safe advice.",
            Color = "#2E7D32"
        });
        Agents.Add(new Agent
        {
            Name = "Travel Planner",
            Description = "Plans trips, itineraries and recommends destinations.",
            Behaviour = "You are a travel planner. Provide detailed itineraries with budget tips.",
            Color = "#0288D1"
        });
        Agents.Add(new Agent
        {
            Name = "Study Buddy",
            Description = "Helps students revise topics and quiz themselves.",
            Behaviour = "You are a study tutor. Explain concepts simply and quiz the user.",
            Color = "#EF6C00"
        });

        LibraryItems.Add(new LibraryItem { Title = "Mediterranean cruise itinerary", Type = "Itinerary", Date = DateTime.Now.AddDays(-2) });
        LibraryItems.Add(new LibraryItem { Title = "Weekly meal plan (vegetarian)", Type = "Meal plan", Date = DateTime.Now.AddDays(-5) });
        LibraryItems.Add(new LibraryItem { Title = "Unit testing cheat sheet", Type = "Notes", Date = DateTime.Now.AddDays(-9) });

        CurrentChat = NewChat();
        ChatHistory.Add(CurrentChat);
    }

    /// <summary>
    /// Creates a new chat session and makes it current.
    /// </summary>
    public ChatSession NewChat(string? agentId = null)
    {
        var agent = Agents.FirstOrDefault(a => a.Id == agentId);
        CurrentChat = new ChatSession
        {
            AgentId = agentId,
            Title = agent != null ? agent.Name : "New chat",
            CreatedDate = DateTime.Now,
            LastActiveDate = DateTime.Now
        };
        return CurrentChat;
    }
}

/// <summary>
/// An item shown on the Library page.
/// </summary>
public class LibraryItem
{
    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = "Notes";

    public DateTime Date { get; set; } = DateTime.Now;
}
