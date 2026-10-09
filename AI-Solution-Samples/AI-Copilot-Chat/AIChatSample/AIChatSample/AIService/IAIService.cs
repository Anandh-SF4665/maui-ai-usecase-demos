namespace AIChatSample.AIService;

/// <summary>
/// Single seam between the UI and any AI provider (NFR-2).
/// Implementations must:
///   - Be safe to call from the UI thread but do I/O on a background thread.
///   - Honour <paramref name="ct"/> for cancellation (NFR-8).
///   - Never embed credentials; read them from environment / user-secrets (NFR-5).
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Sends a user prompt to the AI provider. The optional
    /// <paramref name="systemContext"/> is used as the system prompt
    /// (e.g. an agent's <c>Behaviour</c> — FR-0.3).
    /// </summary>
    /// <returns>
    /// A <see cref="ContextSuggestionResponse"/> with the assistant's
    /// answer and any follow-up suggestions, or <c>null</c> if the
    /// request was cancelled.
    /// </returns>
    Task<ContextSuggestionResponse?> GetResponseAsync(
        string prompt,
        string? systemContext = null,
        CancellationToken ct = default);
}
