namespace AIChatSample.AIService
{
    public interface IAzureAIService
    {
        Task<ContextSuggestionResponse?> GetResponseAsync(string prompt);
    }
}
