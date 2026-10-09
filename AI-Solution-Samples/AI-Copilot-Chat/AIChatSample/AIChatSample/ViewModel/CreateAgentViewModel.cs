using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AIChatSample.Models;
using AIChatSample.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample.ViewModel;


public partial class CreateAgentViewModel : ObservableObject
{
    private readonly IAgentStore? _agentStore;

    public CreateAgentViewModel(IAgentStore? agentStore = null)
    {
        this.EnableSendIcon = true;
        this.IsHeaderVisible = true;
        this.IsEditorVisible = true;

        this.ModelOptions = new ObservableCollection<string> { "Model-1", "Model-2", "Model-3" };
        this.SelectedModel = "GPT-4o";

        this.Suggestions = new ObservableCollection<ISuggestion>
        {
            new TemplateCardSuggestion
            {
                Text = "Research agent",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Research agent",
                DescriptionText = "Finds and synthesizes information from multiple sources automatically.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
            new TemplateCardSuggestion
            {
                Text = "Customer support agent",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Customer support agent",
                DescriptionText = "Handles common inquiries and routes complex issues to your team.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
            new TemplateCardSuggestion
            {
                Text = "Data analysis agent",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Data analysis agent",
                DescriptionText = "Interprets datasets, spots trends, and generates visual reports.",
                Section = CardSection.Template,
                IsAgentTemplate = true,
            },
        };

        this.MyAgents = new ObservableCollection<TemplateCardSuggestion>
        {
            new()
            {
                Text = "Meeting notes assistant",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Meeting notes assistant",
                DescriptionText = "Turns raw meeting transcripts into structured action items.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
            new()
            {
                Text = "Code reviewer",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Code reviewer",
                DescriptionText = "Reviews pull requests and suggests improvements to your code.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
            new()
            {
                Text = "Social media writer",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                TitleText = "Social media writer",
                DescriptionText = "Creates engaging posts tailored to each platform's audience.",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            },
        };

        this.HeaderItemTappedCommand = new Command<object>(this.OnCardTapped);
        this.SendButtonCommand = new Command(this.OnSendRequested);
        this.AttachmentButtonCommand = new Command<object>(_ => Debug.WriteLine("[Attach] tapped"));
        this.VoiceCommand = new Command(() => Debug.WriteLine("[Voice] tapped"));

        // Keep the "My Agents" row in sync with created agents.
        _agentStore = agentStore;
        if (_agentStore is not null)
        {
            SyncAgentsFromStore();
            _agentStore.AgentAdded += OnAgentAddedToStore;
        }
    }

    /// <summary>Rebuilds the MyAgents collection from the shared store.</summary>
    public void SyncAgentsFromStore()
    {
        if (_agentStore is null)
        {
            return;
        }

        foreach (var agent in _agentStore.Agents)
        {
            if (this.MyAgents.Any(a => string.Equals(a.TitleText, agent.AgentName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            this.MyAgents.Insert(0, new TemplateCardSuggestion
            {
                Text = agent.AgentName,
                TitleText = agent.AgentName,
                DescriptionText = agent.AgentDescription ?? "Custom specialist agent.",
                IconGlyph = "\ue736",
                IconFontFamily = "MaterialAssets",
                IconBackground = "#F1ECFE",
                Section = CardSection.MyAgents,
                IsAgentTemplate = true,
            });
        }
    }

    private void OnAgentAddedToStore(object? sender, Models.AgentConfiguration e) => SyncAgentsFromStore();

    #region Properties

    /// <summary>Options exposed by the model-selector ComboBox in the header.</summary>
    public ObservableCollection<string> ModelOptions { get; }

    /// <summary>Active chat messages surfaced in <see cref="SfAIAssistView"/>.</summary>
    public ObservableCollection<IAssistItem> Messages { get; } = new();

    /// <summary>Cards fed to <see cref="SfAIAssistView.Suggestions"/>.</summary>
    public ObservableCollection<ISuggestion> Suggestions { get; }

    /// <summary>My Agents cards rendered in a separate SfListView below.</summary>
    public ObservableCollection<TemplateCardSuggestion> MyAgents { get; }

    [ObservableProperty] private string? promptText;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string selectedModel = "Model";

    [ObservableProperty] private bool enableSendIcon;
    [ObservableProperty] private bool isHeaderVisible;
    [ObservableProperty] private bool isEditorVisible;

    #endregion

    #region Commands

    public ICommand HeaderItemTappedCommand { get; }

    public ICommand SendButtonCommand { get; }
    public ICommand AttachmentButtonCommand { get; }
    public ICommand VoiceCommand { get; }

    #endregion

    #region Command handlers

    private void OnCardTapped(object? parameter)
    {
        if (parameter is not SuggestionItemSelectedEventArgs args || args.SelectedItem is not TemplateCardSuggestion card)
        {
            return;
        }

        var request = new AssistItem
        {
            Text = card.Text ?? string.Empty,
            IsRequested = true,
        };
        this.Messages.Add(request);

        if (card.Section == CardSection.Template)
        {
            this.Suggestions.Clear();
            this.IsHeaderVisible = false;
        }
    }

    private void OnSendRequested()
    {
        if (string.IsNullOrWhiteSpace(this.PromptText))
        {
            return;
        }

        var request = new AssistItem
        {
            Text = this.PromptText!,
            IsRequested = true,
        };
        this.Messages.Add(request);
        this.PromptText = string.Empty;
    }

    #endregion
}