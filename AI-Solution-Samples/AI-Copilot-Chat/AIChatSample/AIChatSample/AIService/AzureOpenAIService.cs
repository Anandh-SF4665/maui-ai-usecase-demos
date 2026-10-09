using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIChatSample.AIService;

/// <summary>
/// Real Azure OpenAI implementation of <see cref="IAIService"/>.
///
/// Endpoints, keys, and deployment names are read from environment
/// variables (which may be supplied through user-secrets in dev) — never
/// from source. Implements NFR-5 and NFR-8.
/// </summary>
public sealed class AzureOpenAIService : IAIService
{
    private const string DefaultSystemPrompt =
"""
You are an AI learning assistant.

Return JSON only.

{
  "answer":"response",
  "suggestions":[
     "suggestion1",
     "suggestion2",
     "suggestion3",
     "suggestion4"
  ]
}

Suggestions must:

1. Be related to the response.
2. Help users continue learning.
3. Never repeat previous suggestions.
4. Be less than 4 words each.
5. Generate exactly 4 suggestions.
""";

    private readonly ChatClient _chatClient;
    private readonly TimeSpan _requestTimeout;

    /// <summary>
    /// Builds the service from the Azure OpenAI environment. Required
    /// variables: <c>AZURE_OPENAI_ENDPOINT</c>, <c>AZURE_OPENAI_KEY</c>,
    /// <c>AZURE_OPENAI_DEPLOYMENT</c>. Optional:
    /// <c>AZURE_OPENAI_TIMEOUT_SECONDS</c> (default 60).
    /// </summary>
    public AzureOpenAIService()
        : this(
            endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
            apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"),
            deployment: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
            timeoutSeconds: ParseInt(Environment.GetEnvironmentVariable("AZURE_OPENAI_TIMEOUT_SECONDS"), defaultValue: 60))
    {
    }

    /// <summary>
    /// Test-friendly constructor that accepts credentials explicitly. The
    /// production constructor pulls from the environment so secrets never
    /// live in source (NFR-5).
    /// </summary>
    public AzureOpenAIService(string? endpoint, string? apiKey, string? deployment, int timeoutSeconds = 60)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_ENDPOINT is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_KEY is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        if (string.IsNullOrWhiteSpace(deployment))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_DEPLOYMENT is not set. Configure it via environment variables or user-secrets before using AzureOpenAIService.");
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deployment);
        _requestTimeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
    }

    public async Task<ContextSuggestionResponse?> GetResponseAsync(
        string prompt,
        string? systemContext = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return new ContextSuggestionResponse { Answer = string.Empty };
        }

        try
        {
            // Combine the caller's cancellation with our hard timeout so the
            // UI never blocks indefinitely (NFR-8).
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_requestTimeout);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(string.IsNullOrWhiteSpace(systemContext) ? DefaultSystemPrompt : systemContext),
                new UserChatMessage(prompt),
            };

            ChatCompletion completion = await _chatClient
                .CompleteChatAsync(messages, cancellationToken: cts.Token)
                .ConfigureAwait(false);

            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (completion?.Content is null || completion.Content.Count == 0)
            {
                return new ContextSuggestionResponse { Answer = string.Empty };
            }

            string result = completion.Content[0].Text ?? string.Empty;
            return ParseResponse(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return new ContextSuggestionResponse
            {
                Answer = $"Request timed out after {_requestTimeout.TotalSeconds:n0} seconds. Check your internet connection and Azure endpoint configuration."
            };
        }
        catch (Exception ex)
        {
            return new ContextSuggestionResponse
            {
                Answer = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Tries to extract a JSON object from the model's reply. The model is
    /// asked to return JSON only, but it sometimes wraps the JSON in
    /// markdown fences or extra text. Falls back to the raw text so the
    /// UI always has something to show.
    /// </summary>
    internal static ContextSuggestionResponse ParseResponse(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return new ContextSuggestionResponse { Answer = string.Empty };
        }

        string json = ExtractFirstJsonObject(result);
        if (string.IsNullOrEmpty(json))
        {
            return new ContextSuggestionResponse { Answer = result };
        }

        try
        {
            return JsonSerializer.Deserialize<ContextSuggestionResponse>(json) ?? new ContextSuggestionResponse { Answer = result };
        }
        catch (JsonException)
        {
            return new ContextSuggestionResponse { Answer = result };
        }
    }

    private static string ExtractFirstJsonObject(string text)
    {
        int firstBrace = text.IndexOf('{');
        int lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return text.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        var stripped = text
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        firstBrace = stripped.IndexOf('{');
        lastBrace = stripped.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return stripped.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return string.Empty;
    }

    private static int ParseInt(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) ? parsed : defaultValue;
}
