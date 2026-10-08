using System.Collections.ObjectModel;
using Syncfusion.Maui.AIAssistView;

namespace CopilotChat.Models;

/// <summary>
/// Represents a user-created AI agent.
/// </summary>
public class Agent
{
    public string Id { get; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Behaviour { get; set; } = string.Empty;

    /// <summary>Background color used for the agent avatar (hex).</summary>
    public string Color { get; set; } = "#6750A4";

    /// <summary>Short glyph/text shown inside the avatar.</summary>
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpperInvariant();

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Represents a single chat conversation.
/// </summary>
public class ChatSession
{
    public string Id { get; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = "New chat";

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime LastActiveDate { get; set; } = DateTime.Now;

    /// <summary>Agent id the chat is associated with; null = default Copilot.</summary>
    public string? AgentId { get; set; }

    public ObservableCollection<IAssistItem> Messages { get; } = new();
}

/// <summary>
/// Represents the signed-in user profile.
/// </summary>
public class UserProfile
{
    public string Name { get; set; } = "Alex Morgan";

    public string Email { get; set; } = "alex.morgan@syncfusion.com";

    public string Color { get; set; } = "#6750A4";
}
