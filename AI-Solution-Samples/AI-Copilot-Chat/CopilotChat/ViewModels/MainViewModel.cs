using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CopilotChat.Models;
using CopilotChat.Services;
using CopilotChat.Views;
using Syncfusion.Maui.AIAssistView;

namespace CopilotChat.ViewModels;

/// <summary>
/// Root view model driving the Copilot-style shell: navigation, agents, chat history and the chat itself.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ChatDataService dataService;
    private readonly IAIService aiService;

    [ObservableProperty]
    private bool isNavOpen = true;

    [ObservableProperty]
    private string currentView = "Chat"; // Chat | Search | Library

    [ObservableProperty]
    private ObservableCollection<ChatSession> chatHistory;

    [ObservableProperty]
    private ObservableCollection<Agent> agents;

    [ObservableProperty]
    private ObservableCollection<IAssistItem> messages;

    [ObservableProperty]
    private ObservableCollection<ISuggestion> suggestions;

    [ObservableProperty]
    private ChatSession currentChat;

    [ObservableProperty]
    private Agent? currentAgent;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatSession> searchResults = new();

    public UserProfile Profile => dataService.Profile;

    public ICommand NewChatCommand { get; }

    public ICommand OpenChatCommand { get; }

    public ICommand OpenAgentCommand { get; }

    public ICommand ShowSearchCommand { get; }

    public ICommand ShowLibraryCommand { get; }

    public ICommand ShowChatCommand { get; }

    public ICommand ToggleNavCommand { get; }

    public ICommand EditProfileCommand { get; }

    public ICommand CreateAgentCommand { get; }

    public MainViewModel(ChatDataService dataService, IAIService aiService)
    {
        this.dataService = dataService;
        this.aiService = aiService;

        ChatHistory = dataService.ChatHistory;
        Agents = dataService.Agents;
        Messages = dataService.CurrentChat.Messages;
        CurrentChat = dataService.CurrentChat;

        NewChatCommand = new Command(() => StartNewChat(null));
        OpenChatCommand = new Command<ChatSession>(session =>
        {
            if (session is null)
            {
                return;
            }

            CurrentChat = session;
            dataService.CurrentChat = session;
            Messages = session.Messages;
            CurrentAgent = Agents.FirstOrDefault(a => a.Id == session.AgentId);
            CurrentView = "Chat";
        });
        OpenAgentCommand = new Command<Agent>(agent =>
        {
            if (agent is null)
            {
                return;
            }

            StartNewChat(agent.Id);
        });
        ShowSearchCommand = new Command(() => CurrentView = "Search");
        ShowLibraryCommand = new Command(() => CurrentView = "Library");
        ShowChatCommand = new Command(() => CurrentView = "Chat");
        ToggleNavCommand = new Command(() => IsNavOpen = !IsNavOpen);
        EditProfileCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ProfilePage)));
        CreateAgentCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(CreateAgentPage)));

        LoadSuggestions();
    }

    /// <summary>
    /// Command bound to the AIAssistView; handles user requests.
    /// </summary>
    public ICommand RequestCommand => new Command<object>(async obj =>
    {
        var prompt = ExtractPrompt(obj);
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        // Give the new chat a title from the first prompt.
        if (CurrentChat.Messages.Count == 0 && CurrentChat.Title == "New chat")
        {
            CurrentChat.Title = prompt.Length > 40 ? prompt[..40] + "…" : prompt;
        }

        var systemPrompt = CurrentAgent != null
            ? CurrentAgent.Behaviour
            : "You are a helpful, concise AI assistant.";

        var response = await aiService.GetResponseAsync(prompt, systemPrompt);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!string.IsNullOrEmpty(response))
            {
                Messages.Add(new AssistItem { Text = response });
            }
            else
            {
                Messages.Add(new AssistItem { Text = "Sorry, I could not generate a response. Please try again." });
            }
        });
    });

    partial void OnSearchQueryChanged(string value)
    {
        SearchResults.Clear();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        foreach (var session in ChatHistory.Where(s => s.Title.Contains(value, StringComparison.OrdinalIgnoreCase)))
        {
            SearchResults.Add(session);
        }
    }

    /// <summary>
    /// Starts a fresh chat session, optionally tied to a specific agent.
    /// </summary>
    public void StartNewChat(string? agentId)
    {
        var chat = dataService.NewChat(agentId);
        if (!ChatHistory.Contains(chat))
        {
            ChatHistory.Insert(0, chat);
        }

        CurrentChat = chat;
        Messages = chat.Messages;
        CurrentAgent = Agents.FirstOrDefault(a => a.Id == agentId);
        CurrentView = "Chat";
        LoadSuggestions();
    }

    private void LoadSuggestions()
    {
        Suggestions = new ObservableCollection<ISuggestion>
        {
            new AssistSuggestion { Text = "Summarize my day" },
            new AssistSuggestion { Text = "Draft an email" },
            new AssistSuggestion { Text = "Plan a trip" },
            new AssistSuggestion { Text = "Help me code" },
        };
    }

    private static string ExtractPrompt(object? obj)
    {
        return obj switch
        {
            ISuggestion s => s.Text ?? string.Empty,
            string str => str,
            IAssistItem item => item.Text ?? string.Empty,
            _ => string.Empty
        };
    }
}
