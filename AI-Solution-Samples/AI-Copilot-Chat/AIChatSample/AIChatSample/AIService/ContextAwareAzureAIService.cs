namespace AIChatSample.AIService;

/// <summary>
/// Backward-compatible adapter that preserves the
/// <see cref="IAzureAIService.GetResponseAsync(string)"/> signature
/// while routing the call through the new <see cref="IAIService"/>
/// abstraction (NFR-2). New code should depend on
/// <see cref="IAIService"/> directly.
/// </summary>
public class ContextAwareAzureAIService : IAzureAIService
{
    private readonly IAIService _inner;

    public ContextAwareAzureAIService(IAIService inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Task<ContextSuggestionResponse?> GetResponseAsync(string prompt)
        => _inner.GetResponseAsync(prompt, systemContext: null, ct: CancellationToken.None);
}
