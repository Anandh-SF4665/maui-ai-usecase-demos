using AIChatSample.Models;

namespace AIChatSample.Services;

/// <summary>
/// Creation/validation gateway for agents. Currently simulates a backend
/// round-trip; replace the body with an Azure AI / REST call when ready.
/// </summary>
public interface IAgentService
{
    Task<bool> ValidateNameNotTakenAsync(string name, CancellationToken ct = default);

    Task<AgentConfiguration> CreateAgentAsync(AgentConfiguration config, CancellationToken ct = default);
}

public sealed class DefaultAgentService : IAgentService
{
    private readonly IAgentStore _store;

    public DefaultAgentService(IAgentStore store) => _store = store;

    public Task<bool> ValidateNameNotTakenAsync(string name, CancellationToken ct = default)
        => Task.FromResult(!_store.IsNameTaken(name));

    public async Task<AgentConfiguration> CreateAgentAsync(AgentConfiguration config, CancellationToken ct = default)
    {
        await Task.Delay(600, ct).ConfigureAwait(false); // simulated network latency

        if (_store.IsNameTaken(config.AgentName ?? string.Empty))
        {
            throw new InvalidOperationException($"An agent named '{config.AgentName}' already exists.");
        }

        return config;
    }
}