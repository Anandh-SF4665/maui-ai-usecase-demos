namespace AIChatSample.AIService;

/// <summary>
/// Deprecated. Use <see cref="IAIService"/> instead.
/// This interface is retained only for backward compatibility.
/// </summary>
[Obsolete("Use IAIService instead. This interface will be removed in a future version.", false)]
public interface IAzureAIService
{
    Task<ContextSuggestionResponse?> GetResponseAsync(string prompt);
}
