using AIChatSample.Models;
using AIChatSample.Models;
using AIChatSample.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample.ViewModel
{
    public partial class AIMainLayoutViewModel : ObservableObject
    {
        #region Fields
        public CreateAgentViewModel CreateAgentVM { get; }
        public AIChatNavigationViewModel chatVM { get; }
        public AgentConfigurationViewModel AgentVM { get; }
        public ProfileSettingsViewModel ProfileVM { get; }
        public UserProfile UserProfile { get; }
        public LibraryViewModel LibraryVM { get; }
        public ImageLibraryViewModel ImageLibraryVM { get; }
        public AgentLogoPopupViewModel AgentPopUpVM { get; }

        // Sidebar / drawer state
        private bool isDrawerOpen;
        private bool isSecondaryDrawerOpen;
        private bool isAgentSectionExpanded = true;
        private bool isRecentChatSectionExpanded = true;

        // Selection state
        private SidebarItem? selectedMenuItem;
        private AgentItem? selectedAgent;

        private string userName = "Alexa John";
        private string userAvatar = "userprofile.png";
        private string profileName = "Alexa John";
        private string profileEmail = "alex.dev@example.com";
        private string profileErrorMessage = string.Empty;

        // Commands
        private ICommand? toggleSidebarCommand;
        private ICommand? openMessagesCommand;
        private ICommand? closeSecondaryDrawerCommand;
        private ICommand? toggleAgentSectionCommand;
        private ICommand? toggleRecentChatSectionCommand;
        private ICommand? menuItemSelectedCommand;
        private ICommand? agentSelectedCommand;
        private ICommand? newChatCommand;
        private ICommand? openChatCommand;
        private ICommand? openAgentChatCommand;
        private ICommand? searchCommand;
        private ICommand? libraryCommand;
        private ICommand? newAgentCommand;
        private ICommand? profileCommand;
        private ICommand? expandCommand;
        private ICommand? configureCommand;
        private ICommand? createAgentCommand;
        private string selectedModel = "Model-1";

        /// <summary>Shared in-memory store (NFR-3, ARCHITECTURE §5). May be null in design-time contexts.</summary>
        private readonly IChatDataService? _chatDataService;

        /// <summary>Shared in-memory agent store. Surfaces new agents so the nav-drawer list and "My Agents" row stay in sync (FR-1.4).</summary>
        private readonly IAgentStore? _agentStore;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AIMainLayoutViewModel"/> class
        /// and seeds the sidebar with default menu items, agents, and recent chats.
        /// </summary>
        public AIMainLayoutViewModel()
        {
            // Resolve from the app service provider when running; fall back
            // to isolated in-memory implementations for design/test contexts.
            var store = ServiceHelper.GetService<IAgentStore>() ?? new InMemoryAgentStore();
            var agentService = ServiceHelper.GetService<IAgentService>() ?? new DefaultAgentService(store);
            _chatDataService = ServiceHelper.GetService<IChatDataService>();
            _agentStore = store;

            CreateAgentVM = new CreateAgentViewModel(store);
            AgentVM = new AgentConfigurationViewModel(agentService, store);
            chatVM = new AIChatNavigationViewModel();
            ProfileVM = new ProfileSettingsViewModel();
            UserProfile = _chatDataService?.Profile ?? new UserProfile();
            profileName = UserProfile.Name;
            profileEmail = UserProfile.Email;
            LibraryVM = new LibraryViewModel();
            ImageLibraryVM = new ImageLibraryViewModel();
            AgentPopUpVM = new AgentLogoPopupViewModel();
            this.ModelOptions = new ObservableCollection<string> { "Model-1", "Model-2", "Model-3" };

            // FR-0.1 / FR-0.4: surface the user profile (live across the
            // app) and the shared chat history. The Recent chats list
            // re-sorts automatically because the underlying store calls
            // Move() on the observable collection when LastActiveDate
            // changes.
            if (_chatDataService is not null)
            {
                this.UserName = _chatDataService.Profile.Name;
                this.ProfileVM.PropertyChanged += OnProfileVMChanged;
                _chatDataService.Profile.PropertyChanged += OnUserProfileChanged;

                RecentChats = new ObservableCollection<RecentChatItem>(BuildRecentChatItems());
                FilteredRecentChats = new ObservableCollection<RecentChatItem>(RecentChats);
                _chatDataService.ChatSessionsChanged += OnChatSessionsChanged;
            }
            this.MenuItems = new ObservableCollection<SidebarItem>
            {
                new()
                {
                    Title = "New chat",
                    IconGlyph = "\ue70d",
                    IconFontFamily = "MaterialAssets",
                    IconImageSource = new FontImageSource
                    {
                        Glyph = "\ue70d",
                        FontFamily = "MaterialAssets",
                        Color = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#C9C6C8") : Color.FromArgb("#474648"),
                        Size = 18
                    }
                },
                new()
                {
                    Title = "Search",
                    IconGlyph = "\ue715",
                    IconFontFamily = "MaterialAssets",
                    IconImageSource = new FontImageSource
                    {
                        Glyph = "\ue715",
                        FontFamily = "MaterialAssets",
                        Color = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#C9C6C8") : Color.FromArgb("#474648"),
                        Size = 18
                    }
                }
            };

            this.Agents = new ObservableCollection<AgentItem>
            {
                new() { Name = "Code Reviewer",       Glyph = "\ue71C" },
                new() { Name = "Social Media Writer", Glyph = "\ue7B4" },
            };

            // FR-1.4: surface any agents already persisted in the store
            // (in this session; the in-memory store doesn't survive
            // restarts). We prepend so the most recently created agent
            // is closest to the top — matches the "Recent" pattern.
            if (_agentStore is not null)
            {
                foreach (var existing in _agentStore.Agents)
                {
                    AddAgentRow(existing);
                }
                _agentStore.AgentAdded += OnAgentAddedToStore;
            }

            // RecentChats is initialised above when a chat data service is
            // available; otherwise seed three demo entries so the UI
            // remains meaningful in design / XAML-preview contexts.
            if (this.RecentChats is null)
            {
                this.RecentChats = new ObservableCollection<RecentChatItem>
                {
                    new() { Title = "Provide a prompt to create the",   Time = "2m", Glyph = "\ue705" },
                    new() { Title = "What is API driven annotation for",   Time = "2m", Glyph = "\ue705" },
                    new() { Title = "What is API driven annotation for", Time = "1h", Glyph = "\ue705" },
                };
            }

            this.ModelOptions = new ObservableCollection<string>
            {
                "Model-1",
                "Model-2",
                "Model-3",
            };
            Application.Current!.RequestedThemeChanged += OnRequestedThemeChanged;
            this.SelectedMenuItem = this.MenuItems.FirstOrDefault();
            this.SelectedAgent = this.Agents.FirstOrDefault();
        }

        #region Properties
        private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            UpdateMenuItemColors(e.RequestedTheme);
        }
        private void UpdateMenuItemColors(AppTheme theme)
        {
            var iconColor = theme == AppTheme.Dark
                ? Color.FromArgb("#C9C6C8")
                : Color.FromArgb("#474648");

            foreach (var item in MenuItems)
            {
                item.IconImageSource = new FontImageSource
                {
                    Glyph = item.IconGlyph,
                    FontFamily = item.IconFontFamily,
                    Color = iconColor,
                    Size = 18
                };
            }
        }
        /// <summary>Top-level menu items shown in the primary drawer.</summary>
        public ObservableCollection<SidebarItem> MenuItems { get; }

        /// <summary>Agents shown in the "Agent" accordion section.</summary>
        public ObservableCollection<AgentItem> Agents { get; }

        /// <summary>Recently visited chats shown in the "Recent chat" accordion section. Backed by <see cref="IChatDataService"/> when available (FR-0.4).</summary>
        public ObservableCollection<RecentChatItem> RecentChats { get; private set; } = new();

        /// <summary>Filtered view of <see cref="RecentChats"/> driven by <see cref="SearchText"/>. FR-4.1.</summary>
        public ObservableCollection<RecentChatItem> FilteredRecentChats { get; private set; } = new();

        /// <summary>
        /// Free-text query bound to the Search page's input. Re-filters
        /// <see cref="FilteredRecentChats"/> on every keystroke (FR-4.1).
        /// </summary>
        [ObservableProperty]
        private string? searchText;

        partial void OnSearchTextChanged(string? value) => ApplySearchFilter();

        /// <summary>Options exposed by the model selector dropdown in the header.</summary>
        public ObservableCollection<string> ModelOptions { get; }

        /// <summary>Gets or sets a value indicating whether the primary drawer is open.</summary>
        public bool IsDrawerOpen
        {
            get => this.isDrawerOpen;
            set => this.SetProperty(ref this.isDrawerOpen, value);
        }

        /// <summary>Gets or sets a value indicating whether the secondary (right) drawer is open.</summary>
        public bool IsSecondaryDrawerOpen
        {
            get => this.isSecondaryDrawerOpen;
            set => this.SetProperty(ref this.isSecondaryDrawerOpen, value);
        }

        public bool IsAgentSectionExpanded
        {
            get => this.isAgentSectionExpanded;
            set => this.SetProperty(ref this.isAgentSectionExpanded, value);
        }

        public bool IsRecentChatSectionExpanded
        {
            get => this.isRecentChatSectionExpanded;
            set => this.SetProperty(ref this.isRecentChatSectionExpanded, value);
        }

        public SidebarItem? SelectedMenuItem
        {
            get => this.selectedMenuItem;
            set
            {
                if (this.selectedMenuItem == value) return;
                this.selectedMenuItem = value;
                this.OnPropertyChanged(nameof(this.SelectedMenuItem));
                this.OnPropertyChanged(nameof(this.HasSelectedMenuItem));
            }
        }

        public AgentItem? SelectedAgent
        {
            get => this.selectedAgent;
            set
            {
                if (this.selectedAgent == value) return;
                this.selectedAgent = value;
                this.OnPropertyChanged(nameof(this.SelectedAgent));
            }
        }

        public string SelectedModel
        {
            get => this.selectedModel;
            set => this.SetProperty(ref this.selectedModel, value);
        }

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value);
        }

        public string UserAvatar
        {
            get => this.userAvatar;
            set => this.SetProperty(ref this.userAvatar, value);
        }

        public string ProfileName
        {
            get => this.profileName;
            set => this.SetProperty(ref this.profileName, value);
        }

        public string ProfileEmail
        {
            get => this.profileEmail;
            set => this.SetProperty(ref this.profileEmail, value);
        }

        public string ProfileErrorMessage
        {
            get => this.profileErrorMessage;
            set => this.SetProperty(ref this.profileErrorMessage, value);
        }

        /// <summary>True when a menu item has been selected (used by drawer isVisible / chevron).</summary>
        public bool HasSelectedMenuItem => this.selectedMenuItem is not null;

        #endregion

        #region Commands

        /// <summary>Toggles the primary (left) navigation drawer open / closed.</summary>
        public ICommand ToggleSidebarCommand =>
            this.toggleSidebarCommand ??= new Command(() => this.IsDrawerOpen = !this.IsDrawerOpen);

        /// <summary>Opens the secondary (right) messages drawer.</summary>
        public ICommand OpenMessagesCommand =>
            this.openMessagesCommand ??= new Command(() => this.IsSecondaryDrawerOpen = true);

        /// <summary>Closes the secondary drawer.</summary>
        public ICommand CloseSecondaryDrawerCommand =>
            this.closeSecondaryDrawerCommand ??= new Command(() => this.IsSecondaryDrawerOpen = false);

        /// <summary>Expands / collapses the Agent accordion section.</summary>
        public ICommand ToggleAgentSectionCommand =>
            this.toggleAgentSectionCommand ??= new Command(() => this.IsAgentSectionExpanded = !this.IsAgentSectionExpanded);

        /// <summary>Expands / collapses the Recent chat accordion section.</summary>
        public ICommand ToggleRecentChatSectionCommand =>
            this.toggleRecentChatSectionCommand ??= new Command(() => this.IsRecentChatSectionExpanded = !this.IsRecentChatSectionExpanded);

        /// <summary>Receives the user's menu selection and updates <see cref="SelectedMenuItem"/>.</summary>
        public ICommand MenuItemSelectedCommand =>
            this.menuItemSelectedCommand ??= new Command<SidebarItem?>(this.OnMenuItemSelected);

        /// <summary>Receives the user's agent selection and updates <see cref="SelectedAgent"/>.</summary>
        public ICommand AgentSelectedCommand =>
            this.agentSelectedCommand ??= new Command<AgentItem?>(this.OnAgentSelected);

        /// <summary>Creates a new chat session and raises <see cref="ChatSessionReplaced"/> so the host page can swap the active conversation (FR-0.4).</summary>
        public ICommand NewChatCommand =>
            this.newChatCommand ??= new Command(this.OnNewChatClicked);

        /// <summary>Opens an existing chat session (parameter: the <see cref="RecentChatItem"/> tapped by the user). Raises <see cref="ChatSessionReplaced"/>.</summary>
        public ICommand OpenChatCommand =>
            this.openChatCommand ??= new Command<RecentChatItem?>(this.OnOpenChatClicked);

        /// <summary>Opens an existing chat session (parameter: the <see cref="RecentChatItem"/> tapped by the user). Raises <see cref="ChatSessionReplaced"/>.</summary>
        public ICommand OpenAgentChatCommand =>
            this.openAgentChatCommand ??= new Command<RecentChatItem?>(this.OnOpenChatClicked);

        /// <summary>Opened or created chat session. The host page (NewChat) listens to this and binds the new conversation into the AI AssistView (FR-0.4).</summary>
        public event EventHandler<ChatSession>? ChatSessionReplaced;

        /// <summary>Currently open chat session, or null when the host should display the welcome header (FR-0.4).</summary>
        public ChatSession? CurrentSession { get; private set; }

        /// <summary>Opens the search panel.</summary>
        public ICommand SearchCommand =>
            this.searchCommand ??= new Command(this.OnSearchClicked);

        /// <summary>Opens the agent library.</summary>
        public ICommand LibraryCommand =>
            this.libraryCommand ??= new Command(this.OnLibraryClicked);

        /// <summary>Starts the new-agent creation flow.</summary>
        public ICommand NewAgentCommand =>
            this.newAgentCommand ??= new Command(this.OnNewAgentClicked);

        /// <summary>Opens the user profile footer / panel.</summary>
        public ICommand ProfileCommand =>
            this.profileCommand ??= new Command(this.OnProfileClicked);

        /// <summary>Toggles the immersive / full-screen layout.</summary>
        public ICommand ExpandCommand =>
            this.expandCommand ??= new Command(this.OnExpandClicked);

        /// <summary>Switches the Create Agent page to the Configure tab.</summary>
        public ICommand ConfigureCommand =>
            this.configureCommand ??= new Command(this.OnConfigureClicked);

        /// <summary>Persists the new specialist agent.</summary>
        public ICommand CreateAgentCommand =>
            this.createAgentCommand ??= new Command(this.OnCreateAgentClicked);

        public ICommand SaveProfileCommand => new Command(this.OnSaveProfileClicked);

        #endregion

        #region Methods

        private void OnMenuItemSelected(SidebarItem? item)
        {
            if (item is null) return;

            foreach (var i in this.MenuItems)
            {
                i.IsSelected = ReferenceEquals(i, item);
            }

            this.SelectedMenuItem = item;
        }

        private void OnAgentSelected(AgentItem? item)
        {
            if (item is null) return;

            foreach (var a in this.Agents)
            {
                a.IsSelected = ReferenceEquals(a, item);
            }

            this.SelectedAgent = item;
        }

        private void OnNewChatClicked()
        {
            // FR-0.4: a fresh chat is a fresh ChatSession. We never mutate
            // an existing session in-place; opening "New chat" from the
            // drawer always creates a new one.
            if (_chatDataService is null) return;
            CurrentSession = _chatDataService.CreateChatSession();
            ChatSessionReplaced?.Invoke(this, CurrentSession);
        }

        private void OnOpenChatClicked(RecentChatItem? item)
        {
            if (item is null || _chatDataService is null) return;

            // Look the session back up by id; RecentChatItem carries the
            // ChatSession.Id under the hood via Tag.
            var session = item.Tag as ChatSession
                ?? _chatDataService.ChatSessions.FirstOrDefault(s => s.Id == item.Tag as string);
            if (session is null) return;

            _chatDataService.TouchSession(session);
            CurrentSession = session;
            ChatSessionReplaced?.Invoke(this, session);
        }

        private void OnOpenAgentChatClicked(AgentItem? item)
        {
            if (item is null || _chatDataService is null || _agentStore is null) return;

            var agent = _agentStore.Agents.FirstOrDefault(a =>
                string.Equals(a.Id, item.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.AgentName, item.Name, StringComparison.OrdinalIgnoreCase));

            if (agent is null)
            {
                return;
            }

            SelectedAgent = item;
            CurrentSession = _chatDataService.CreateChatSession(agent.AgentName, agent);
            ChatSessionReplaced?.Invoke(this, CurrentSession);
        }

        private void OnSearchClicked() { }
        private void OnLibraryClicked() { }
        private void OnNewAgentClicked() { }
        private void OnProfileClicked() { }
        private void OnExpandClicked() { }
        private void OnConfigureClicked() { }
        private void OnCreateAgentClicked() { }

        private void OnSaveProfileClicked()
        {
            var name = (ProfileName ?? string.Empty).Trim();
            var email = (ProfileEmail ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ProfileErrorMessage = "Display name is required.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            {
                ProfileErrorMessage = "Enter a valid email address.";
                return;
            }

            ProfileErrorMessage = string.Empty;
            UserProfile.Name = name;
            UserProfile.Email = email;
            UserName = UserProfile.Name;

            if (_chatDataService is not null)
            {
                _chatDataService.Profile.Name = name;
                _chatDataService.Profile.Email = email;
            }
        }

        private void OnUserProfileChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_chatDataService is null) return;
            // FR-3.3 / FR-0.1: nav header initial/name updates live.
            if (e.PropertyName is nameof(UserProfile.Name) or nameof(UserProfile.Initial))
            {
                UserName = _chatDataService.Profile.Name;
                ProfileName = _chatDataService.Profile.Name;
            }
            else if (e.PropertyName is nameof(UserProfile.Email))
            {
                ProfileEmail = _chatDataService.Profile.Email;
            }
        }

        private void OnProfileVMChanged(object? sender, PropertyChangedEventArgs e)
        {
            // No-op placeholder; T09 mutates UserProfile via SaveProfileCommand.
        }

        private void OnChatSessionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Re-render the Recent chats list whenever the store is
            // mutated (e.g. a new session is created, or TouchSession
            // re-sorts the collection). FR-0.4.
            if (_chatDataService is null) return;
            RecentChats = new ObservableCollection<RecentChatItem>(BuildRecentChatItems());
            OnPropertyChanged(nameof(RecentChats));
            ApplySearchFilter();
        }

        /// <summary>
        /// Inserts a single <see cref="AgentItem"/> row for the freshly
        /// created agent. De-duplicated by id (FR-1.4) and by
        /// case-insensitive name so the seeded design-time defaults
        /// don't appear twice. T07 / T08 will reuse the same row to
        /// surface the customized avatar.
        /// </summary>
        private void AddAgentRow(Models.AgentConfiguration agent)
        {
            if (agent is null || string.IsNullOrWhiteSpace(agent.AgentName))
            {
                return;
            }

            // Skip duplicates seeded at design time (e.g. "Code
            // Reviewer") when the user happens to create one with the
            // same name; the unique-name guarantee in IAgentStore.Add
            // means we never have to deal with two rows for the same
            // persisted record.
            foreach (var existing in Agents)
            {
                if (!string.IsNullOrEmpty(existing.Id) && existing.Id == agent.Id)
                {
                    return;
                }

                if (string.Equals(existing.Name, agent.AgentName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            Agents.Insert(0, new AgentItem
            {
                Id = agent.Id,
                Name = agent.AgentName,
                Initial = agent.Initial ?? Models.AgentConfiguration.DeriveInitial(agent.AgentName),
                Color = agent.Color ?? Models.AgentConfiguration.DeriveColor(agent.AgentName),
                Glyph = "\ue71C",
            });
        }

        /// <summary>
        /// Raised by <see cref="IAgentStore.AgentAdded"/> when a new
        /// specialist agent is persisted. Pushes a row into the
        /// nav-drawer <see cref="Agents"/> list (FR-1.4) and leaves
        /// selection unchanged so the user keeps the current focus.
        /// </summary>
        private void OnAgentAddedToStore(object? sender, Models.AgentConfiguration e)
        {
            AddAgentRow(e);
        }

        private void ApplySearchFilter()
        {
            // FR-4.1: filter the Recent chats list by Title containing
            // SearchText. Empty search restores the unfiltered list.
            if (FilteredRecentChats is null)
            {
                FilteredRecentChats = new ObservableCollection<RecentChatItem>();
            }

            FilteredRecentChats.Clear();
            var query = (SearchText ?? string.Empty).Trim();
            var source = RecentChats;
            foreach (var item in source)
            {
                if (query.Length == 0 ||
                    (item.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    FilteredRecentChats.Add(item);
                }
            }
            OnPropertyChanged(nameof(FilteredRecentChats));
        }

        private IEnumerable<RecentChatItem> BuildRecentChatItems()
        {
            if (_chatDataService is null) yield break;
            foreach (var session in _chatDataService.ChatSessions)
            {
                yield return new RecentChatItem
                {
                    Title = session.Title,
                    Time = FormatRelative(session.LastActiveDate),
                    Glyph = "\ue705",
                    Tag = session,
                };
            }
        }

        private static string FormatRelative(DateTime utc)
        {
            var delta = DateTime.UtcNow - utc.ToUniversalTime();
            if (delta.TotalSeconds < 60) return "now";
            if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes}m";
            if (delta.TotalHours < 24) return $"{(int)delta.TotalHours}h";
            if (delta.TotalDays < 7) return $"{(int)delta.TotalDays}d";
            return utc.ToLocalTime().ToString("MMM d");
        }

        #endregion

        #region INotifyPropertyChanged (kept for backward compat — ObservableObject handles dispatch)

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return;
            storage = value;
            this.OnPropertyChanged(propertyName!);
        }

        #endregion
    }
}