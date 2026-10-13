using AIChatSample.AIService;
using AIChatSample.Models;
using AIChatSample.Services;
using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AIChatSample.ViewModel;

/// <summary>
/// View-model for the new-chat screen. Owns the prompt + response flow
/// (FR-0.2, FR-0.3) and delegates the actual AI call to
/// <see cref="IAIService"/> (NFR-2). Uses async/await so the UI thread
/// stays responsive while a request is in flight (NFR-8).
/// </summary>
public class AIChatSuggestionViewModel : INotifyPropertyChanged
{
    private readonly IAIService _aiService;
    private readonly IChatDataService _data;

    private ObservableCollection<ISuggestion> _suggestions;
    private ObservableCollection<IAssistItem> _assistItems;
    private string _inputText = string.Empty;
    private bool _isBusy;
    private string? _systemContext;
    private ChatSession? _session;
    private CancellationTokenSource? _cts;
    private AssistItem? _activeResponseItem;
    public AIChatSuggestionViewModel()
        : this(
            aiService: ServiceHelper.GetService<IAIService>() ?? new MockAIService(),
            data: ServiceHelper.GetService<IChatDataService>() ?? new ChatDataService())
    {
    }

    public AIChatSuggestionViewModel(IAIService aiService, IChatDataService data)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _data = data ?? throw new ArgumentNullException(nameof(data));

        Color iconColor = Color.FromArgb("#7633DA");
        if (Application.Current != null)
        {
            iconColor = Application.Current.UserAppTheme == AppTheme.Dark
                ? Color.FromArgb("#A572F7")
                : (Color)Application.Current.Resources["PrimaryColorLight"];
        }

        // FR-4.3: initial prompts surfaced as the welcome-state
        // suggestion grid. The list intentionally does not change based
        // on the active agent — these are global starters.
        this._suggestions = new ObservableCollection<ISuggestion>()
        {
            new AssistSuggestion() { Text = "Summarize a document", ImageSource = new FontImageSource() { Glyph = "\ue797", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Analyze business data", ImageSource = new FontImageSource() { Glyph = "\ue793", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Draft content", ImageSource = new FontImageSource() { Glyph = "\ue7f2", FontFamily = "MaterialAssets", Color = iconColor } },
            new AssistSuggestion() { Text = "Solve a technical problem", ImageSource = new FontImageSource() { Glyph = "\ue7f5", FontFamily = "MaterialAssets", Color = iconColor } },
        };

        this._assistItems = new ObservableCollection<IAssistItem>();
        this._assistItems.CollectionChanged += OnAssistItemsChanged;

        // FR-4.3: the follow-up suggestions returned by the AI service
        // (e.g. "Tell me more", "Give an example") are surfaced via the
        // syncfusion AIAssistView's suggestion strip; binding
        // Suggestions above keeps that strip alive.
        _data.ChatSessionsChanged += OnDataSessionsChanged;

        // Agents shown in the AIAssistView agent selector: two defaults
        // plus any agent created by the user (kept in sync via IAgentStore).
        // Icons intentionally use ImageSource (not FontImageSource) on
        // Windows. The default template applies a DropShadow whose mask is
        // a CompositionBrush derived from the icon; FontImageSource reuses
        // a visual that gets disposed when the selector recycles, leading
        // to "object has been closed" on the next selection.
        this._agents = new ObservableCollection<AssistAgent>
        {
            new AssistAgent
            {
                Name = "Code Reviewer",
                Description = "Reviews code for bugs, readability, performance and best practices.",
                Instructions = "You are an expert code reviewer. Analyze the provided code for bugs, security issues, performance problems, readability and best practices. Give concise, actionable feedback with improved code snippets where helpful.",
                Icon = new FontImageSource { Glyph = "\ue71C", FontFamily = "MaterialAssets", Color = iconColor },
            },
            new AssistAgent
            {
                Name = "Social Media Writer",
                Description = "Writes engaging, platform-ready social media posts.",
                Instructions = "You are a creative social media writer. Produce engaging, concise posts tailored to the requested platform and tone, with relevant hashtags and a clear call to action.",
                Icon = new FontImageSource { Glyph = "\ue7B4", FontFamily = "MaterialAssets", Color = iconColor },
            },
        };

        this._agentStore = ServiceHelper.GetService<IAgentStore>();
        if (this._agentStore is not null)
        {
            foreach (var existing in this._agentStore.Agents)
            {
                this.AddAssistAgent(existing);
            }
            this._agentStore.AgentAdded += this.OnAgentStoreAgentAdded;
        }

        this.RequestCommand = new Command<object>(this.OnRequest);
        this.SendCommand = new Command(this.OnSendRequested, this.CanSend);
        this.SuggestionTappedCommand = new Command<ISuggestion>(this.OnSuggestionTapped);
        this.CancelRequestCommand = new Command(this.CancelActiveRequest, () => this._isBusy);
    }

    private readonly IAgentStore? _agentStore;
    private readonly ObservableCollection<AssistAgent> _agents;
    private AssistAgent? _selectedAgent;

    /// <summary>Agents bound to <c>SfAIAssistView.Agents</c>.</summary>
    public ObservableCollection<AssistAgent> Agents => this._agents;

    /// <summary>Agent bound to <c>SfAIAssistView.SelectedAgent</c>; its instructions drive the system context.</summary>
    public AssistAgent? SelectedAgent
    {
        get => this._selectedAgent;
        set
        {
            if (ReferenceEquals(this._selectedAgent, value)) return;
            this._selectedAgent = value;
            this.SetSystemContext(value?.Instructions);
            this.RaisePropertyChanged();
        }
    }

    private void AddAssistAgent(AgentConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.AgentName)
            || this._agents.Any(a => string.Equals(a.Name, config.AgentName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        // Don't set an Icon on Windows. The default selector template
        // applies a DropShadow whose mask is a CompositionBrush sourced
        // from the icon. A FontImageSource-backed brush gets disposed when
        // the selector recycles its visuals, which surfaces as
        // "System.ObjectDisposedException: The object has been closed" the
        // next time the user selects an agent. Pass a file (AvatarSource)
        // if you want a real icon there.
        ImageSource? icon = null;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            if (!string.IsNullOrWhiteSpace(config.AvatarSource))
            {
                icon = ImageSource.FromFile(config.AvatarSource);
            }
        }
        else if (!string.IsNullOrWhiteSpace(config.AvatarSource))
        {
            icon = ImageSource.FromFile(config.AvatarSource);
        }
        else
        {
            Color bg = Color.FromArgb("#7633DA");
            try { if (!string.IsNullOrEmpty(config.Color)) bg = Color.FromArgb(config.Color); } catch { }
            icon = new FontImageSource
            {
                Glyph = config.Initial ?? config.AgentName.Substring(0, 1).ToUpperInvariant(),
                Color = bg,
            };
        }

        this._agents.Add(new AssistAgent
        {
            Name = config.AgentName,
            Description = config.AgentDescription,
            Instructions = config.Instructions,
            Icon = icon,
        });
    }

    private void OnAgentStoreAgentAdded(object? sender, AgentConfiguration config)
    {
        if (MainThread.IsMainThread) this.AddAssistAgent(config);
        else MainThread.BeginInvokeOnMainThread(() => this.AddAssistAgent(config));
    }

    public ObservableCollection<ISuggestion> Suggestions
    {
        get { return this._suggestions; }
        set
        {
            this._suggestions = value;
            this.RaisePropertyChanged(nameof(Suggestions));
        }
    }

    public ObservableCollection<IAssistItem> AssistItems
    {
        get { return this._assistItems; }
        set
        {
            if (this._assistItems != null)
            {
                this._assistItems.CollectionChanged -= OnAssistItemsChanged;
            }

            this._assistItems = value ?? new ObservableCollection<IAssistItem>();
            this._assistItems.CollectionChanged += OnAssistItemsChanged;
            this.RaisePropertyChanged(nameof(AssistItems));
            this.RaisePropertyChanged(nameof(HasMessages));
        }
    }

    /// <summary>True when the user has already started a conversation. Used to hide the suggestion grid.</summary>
    public bool HasMessages => this._assistItems != null && this._assistItems.Count > 0;

    /// <summary>
    /// True when the current session was opened from an agent (either a
    /// default or a user-created one). Drives the header copy and hides
    /// the "Suggested for you" grid so an agent chat opens straight to
    /// the input box.
    /// </summary>
    public bool IsAgentSession => this.CurrentSession?.Agent is not null;

    /// <summary>Header title shown in the welcome state. Falls back to a generic prompt when no agent is active.</summary>
    public string HeaderTitle => this.CurrentSession?.Agent?.AgentName ?? "How can I help you today?";

    /// <summary>Header subtitle shown in the welcome state. Falls back to a generic prompt when no agent is active.</summary>
    public string HeaderSubtitle =>
        !string.IsNullOrWhiteSpace(this.CurrentSession?.Agent?.AgentDescription)
            ? this.CurrentSession!.Agent!.AgentDescription
            : "I am ready to assist with writing, analysis, or answering your questions.";

    private void OnSessionChanged()
    {
        this.RaisePropertyChanged(nameof(IsAgentSession));
        this.RaisePropertyChanged(nameof(HeaderTitle));
        this.RaisePropertyChanged(nameof(HeaderSubtitle));
    }

    public string InputText
    {
        get => this._inputText;
        set
        {
            if (this._inputText == value) return;
            this._inputText = value;
            this.RaisePropertyChanged(nameof(InputText));
            (this.SendCommand as Command)?.ChangeCanExecute();
        }
    }

    public bool IsBusy
    {
        get => this._isBusy;
        set
        {
            if (this._isBusy == value) return;
            this._isBusy = value;
            this.RaisePropertyChanged(nameof(IsBusy));
            (this.CancelRequestCommand as Command)?.ChangeCanExecute();
            (this.SendCommand as Command)?.ChangeCanExecute();
        }
    }

    public ICommand RequestCommand { get; }

    public ICommand SendCommand { get; }

    public ICommand SuggestionTappedCommand { get; }

    public ICommand CancelRequestCommand { get; }

    /// <summary>Currently bound <see cref="ChatSession"/>, or <c>null</c> when showing the welcome state (FR-0.4).</summary>
    public ChatSession? CurrentSession
    {
        get => _session;
        private set
        {
            if (ReferenceEquals(_session, value)) return;
            _session = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(HasSession));
            RaisePropertyChanged(nameof(CurrentAgent));
            this.OnSessionChanged();
        }
    }

    public AgentConfiguration? CurrentAgent => _session?.Agent;

    /// <summary>True when a chat session is bound. Drives the welcome-header visibility on the host page.</summary>
    public bool HasSession => _session is not null;

    /// <summary>
    /// Loads an existing chat session into the view-model, replacing the
    /// current conversation with the session's stored messages (FR-0.4).
    /// </summary>
    public void LoadSession(ChatSession session)
    {
        if (session is null) return;

        CurrentSession = session;
        SetSystemContext(session.Agent?.Instructions);

        // Make sure the session is fresh in the Recent list.
        _data.TouchSession(session);

        _assistItems.Clear();
        foreach (var message in session.Messages)
        {
            _assistItems.Add(message);
        }
    }

    private bool CanSend() => !string.IsNullOrWhiteSpace(this._inputText) && !this._isBusy;

    private void OnSendRequested()
    {
        if (!this.CanSend())
        {
            return;
        }

        // FR-0.4: every new prompt lazily creates a ChatSession so the
        // Recent-chats list reflects what the user typed. This keeps the
        // behaviour consistent whether the user typed into the welcome
        // state or into an existing session.
        if (_session is null)
        {
            var session = _data.CreateChatSession();
            CurrentSession = session;
            SetSystemContext(session.Agent?.Instructions);
        }

        var request = new AssistItem
        {
            Text = this._inputText,
            IsRequested = true,
        };
        this._assistItems.Add(request);
        if (_session is not null && !_session.Messages.Contains(request))
        {
            _session.Messages.Add(request);
        }
        var prompt = request.Text;
        this.InputText = string.Empty;
        TouchSession();

        // The custom input editor bypasses the AssistView's own request
        // flow, so the AI call must be triggered here.
        _ = SendPromptAsync(prompt);
    }

    private async void OnRequest(object? parameter)
    {
        if (parameter is not RequestEventArgs args || args.RequestItem is not IAssistItem userItem)
        {
            return;
        }

        // The SfAIAssistView already added the request item to AssistItems
        // before raising the event. We just need to call the AI service
        // and append the response (FR-0.2 / FR-0.3).
        args.Handled = true;

        var prompt = userItem.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        await SendPromptAsync(prompt).ConfigureAwait(true);
    }

    /// <summary>
    /// Sends a prompt to the AI service on a background thread and
    /// appends the response to the chat surface. Marshals UI updates
    /// back to the main thread (NFR-8).
    /// </summary>
    public async Task SendPromptAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt) || this._isBusy)
        {
            return;
        }

        CancelActiveRequest();
        this.IsBusy = true;
        this.ShowResponseLoader = true;

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            var result = await Task.Run(async () =>
                await _aiService.GetResponseAsync(prompt, _systemContext, ct).ConfigureAwait(false),
                ct).ConfigureAwait(true);

            if (ct.IsCancellationRequested || result is null)
            {
                return;
            }
            AddResponse(result.Answer ?? string.Empty);
        }
        catch (OperationCanceledException)
        {
            // User-cancelled.
        }
        catch (Exception ex)
        {
            AddResponse($"Error: {ex.Message}");
        }
        finally
        {
            this.ShowResponseLoader = false;
            this.IsBusy = false;
            _cts?.Dispose();
            _cts = null;
            TouchSession();
        }
    }

    private void AddResponse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var item = new AssistItem { Text = text, IsRequested = false };
        this.ShowResponseLoader = false;
        this._assistItems.Add(item);
        if (_session is not null && !_session.Messages.Contains(item))
        {
            _session.Messages.Add(item);
        }
    }

    private bool _showResponseLoader;

    /// <summary>Drives SfAIAssistView.ShowResponseLoader while a response is pending.</summary>
    public bool ShowResponseLoader
    {
        get => _showResponseLoader;
        set
        {
            if (_showResponseLoader == value) return;
            _showResponseLoader = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// FR-4.3: replace the static follow-up suggestions on the
    /// AIAssistView with the context-aware ones returned by the AI
    /// service. We don't surface them as additional chat bubbles — the
    /// suggestion strip below the editor is the canonical place.
    /// </summary>
    private void AppendFollowUpSuggestions(IEnumerable<string>? suggestions)
    {
        if (suggestions is null) return;

        var iconColor = Color.FromArgb("#7633DA");
        if (Application.Current != null)
        {
            iconColor = Application.Current.UserAppTheme == AppTheme.Dark
                ? Color.FromArgb("#A572F7")
                : (Color)Application.Current.Resources["PrimaryColorLight"];
        }

        // Replace any existing suggestions so we never accumulate stale
        // ones from a previous turn.
        this._suggestions.Clear();
        foreach (var text in suggestions)
        {
            if (string.IsNullOrWhiteSpace(text)) continue;
            this._suggestions.Add(new AssistSuggestion
            {
                Text = text,
                ImageSource = new FontImageSource { Glyph = "\ue7f5", FontFamily = "MaterialAssets", Color = iconColor },
            });
        }
        this.RaisePropertyChanged(nameof(Suggestions));
    }


    private void CancelActiveRequest()
    {
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed.
        }
    }

    private void TouchSession()
    {
        if (_session is null) return;
        _data.TouchSession(_session);
    }

    private void OnDataSessionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // No-op for now; placeholder for any reactive UI work that
        // depends on the session list (e.g. cross-tab sync).
    }

    private void OnSuggestionTapped(ISuggestion? suggestion)
    {
        if (suggestion is null || string.IsNullOrWhiteSpace(suggestion.Text))
        {
            return;
        }

        if (_session is null)
        {
            var session = _data.CreateChatSession();
            CurrentSession = session;
            SetSystemContext(session.Agent?.Instructions);
        }

        var item = new AssistItem
        {
            Text = suggestion.Text,
            IsRequested = true,
        };
        this._assistItems.Add(item);
        if (_session is not null && !_session.Messages.Contains(item))
        {
            _session.Messages.Add(item);
        }

        _ = SendPromptAsync(suggestion.Text);
    }

    private void OnAssistItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(HasMessages));
    }

    /// <summary>
    /// Clears the current conversation so the AIAssistView renders the
    /// initial welcome state again. Called by <see cref="Views.AIChat.NewChat"/>
    /// when the page appears (for example after the user taps "New chat" in
    /// the navigation drawer).
    /// </summary>
    public void ResetConversation()
    {
        CancelActiveRequest();
        this._inputText = string.Empty;
        this.RaisePropertyChanged(nameof(InputText));
        (this.SendCommand as Command)?.ChangeCanExecute();

        if (this._assistItems.Count == 0)
        {
            return;
        }

        this._assistItems.Clear();
    }

    /// <summary>Sets the system context used for the next prompt (e.g. an agent's Behaviour).</summary>
    public void SetSystemContext(string? systemContext)
    {
        _systemContext = systemContext;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
