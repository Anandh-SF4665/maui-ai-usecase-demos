using System.Collections.ObjectModel;
using AIChatSample.Models;
using AIChatSample.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AIChatSample.ViewModel;

/// <summary>
/// Composite view-model for the right-pane of <c>AgentConfigurePage</c>:
/// exposes the agent-configuration form fields directly so the XAML can
/// bind without a "Wrapper.AgentVM.AgentName" path, while still surfacing
/// the shared sidebar / recent-chats data the page needs.
/// </summary>
public partial class AgentConfigurePageViewModel : ObservableObject
{
    public AgentConfigurePageViewModel(IAgentService agentService, IAgentStore agentStore)
    {
        AgentVM = new AgentConfigurationViewModel(agentService, agentStore);
        ChatLayout = new AIMainLayoutViewModel();

        // Forward agent-form property changes so IsCreateEnabled stays live
        // on this VM too (the Create Agent button binds here).
        AgentVM.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(e.PropertyName);
        };
    }

    /// <summary>Underlying agent-configuration view-model (form + tabs).</summary>
    public AgentConfigurationViewModel AgentVM { get; }

    /// <summary>Sidebar / drawer / recent-chats state shared with the other pages.</summary>
    public AIMainLayoutViewModel ChatLayout { get; }

    // ---- Flattened properties so XAML can bind "AgentName" / "Instructions"
    //      / "SelectedTab" / "IsConfigureMode" / "IsPreviewMode" / etc. directly. ----
    public string? AgentName
    {
        get => AgentVM.AgentName;
        set => AgentVM.AgentName = value;
    }

    public string? Description
    {
        get => AgentVM.Description;
        set => AgentVM.Description = value;
    }

    public string? Instructions
    {
        get => AgentVM.Instructions;
        set => AgentVM.Instructions = value;
    }

    public string? CompanyName
    {
        get => AgentVM.CompanyName;
        set => AgentVM.CompanyName = value;
    }

    public string? AgentAvatar
    {
        get => AgentVM.AgentAvatar;
        set => AgentVM.AgentAvatar = value;
    }

    public int SelectedTab
    {
        get => AgentVM.SelectedTab;
        set => AgentVM.SelectedTab = value;
    }

    public string[] ModeTabs => AgentVM.ModeTabs;
    public bool IsConfigureMode => AgentVM.IsConfigureMode;
    public bool IsPreviewMode => AgentVM.IsPreviewMode;
    public bool HasAvatar => AgentVM.HasAvatar;
    public bool IsCreateEnabled => AgentVM.IsCreateEnabled;
    public bool IsBusy => AgentVM.IsBusy;
    public bool HasError => AgentVM.HasError;
    public string? ErrorMessage => AgentVM.ErrorMessage;
    public bool CreatedSuccessfully => AgentVM.CreatedSuccessfully;

    // ---- Inline name-required validation (FR-1.2 / AC-1.2) ----
    public bool HasNameValidationError => AgentVM.HasNameValidationError;
    public string NameValidationMessage => AgentVM.NameValidationMessage;

    /// <summary>Called by the page when the name <c>Entry</c> gains focus; promotes the inline error (FR-1.2).</summary>
    public void NotifyNameInteracted() => AgentVM.NotifyNameInteracted();
}
