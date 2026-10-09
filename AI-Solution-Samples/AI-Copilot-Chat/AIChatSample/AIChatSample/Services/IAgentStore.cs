namespace AIChatSample.Services;

/// <summary>
/// Persist/track created agents across the app (single source of truth).
/// Swap the in-memory implementation for a backend/Azure-backed one later.
/// </summary>
public interface IAgentStore
{
    IReadOnlyList<Models.AgentConfiguration> Agents { get; }

    /// <summary>Raised on the UI thread when a new agent is added.</summary>
    event EventHandler<Models.AgentConfiguration>? AgentAdded;

    void Add(Models.AgentConfiguration config);

    bool IsNameTaken(string name);
}

public sealed class InMemoryAgentStore : IAgentStore
{
    private readonly List<Models.AgentConfiguration> _agents = new();

    public IReadOnlyList<Models.AgentConfiguration> Agents => _agents;

    public event EventHandler<Models.AgentConfiguration>? AgentAdded;

    public void Add(Models.AgentConfiguration config)
    {
        if (config is null || string.IsNullOrWhiteSpace(config.AgentName))
        {
            throw new ArgumentException("Agent name is required.", nameof(config));
        }

        if (IsNameTaken(config.AgentName))
        {
            throw new InvalidOperationException($"An agent named '{config.AgentName}' already exists.");
        }

        _agents.Add(config);
        AgentAdded?.Invoke(this, config);
    }

    public bool IsNameTaken(string name) => _agents.Any(a => string.Equals(a.AgentName, name?.Trim(), StringComparison.OrdinalIgnoreCase));
}