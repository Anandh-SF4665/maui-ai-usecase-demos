using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace CopilotChat;

/// <summary>
/// Abstraction over the AI backend used by the chat.
/// </summary>
public interface IAIService
{
    Task<string?> GetResponseAsync(string prompt, string? systemPrompt = null);
}

/// <summary>
/// Azure OpenAI implementation of <see cref="IAIService"/>.
/// Reads endpoint, API key and deployment name from environment variables:
/// AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT.
/// </summary>
public class AzureOpenAIService : IAIService
{
    private const string DefaultSystemPrompt = "You are a helpful, concise AI assistant.";

    public async Task<string?> GetResponseAsync(string prompt, string? systemPrompt = null)
    {
        try
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                return "AI service is not configured. Set AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY and AZURE_OPENAI_DEPLOYMENT to enable live responses.";
            }

            var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            ChatClient chatClient = client.GetChatClient(deployment);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt ?? DefaultSystemPrompt),
                new UserChatMessage(prompt)
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cts.Token);

            return completion.Content.Count > 0 ? completion.Content[0].Text : null;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
