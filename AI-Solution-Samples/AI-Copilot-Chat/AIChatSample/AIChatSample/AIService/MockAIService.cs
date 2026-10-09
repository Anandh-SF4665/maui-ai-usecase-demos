using System.Text;

namespace AIChatSample.AIService;

/// <summary>
/// Deterministic offline implementation of <see cref="IAIService"/> used
/// when no Azure OpenAI configuration is supplied. Lets the sample run
/// with zero configuration (NFR-2) and supports the harness's per-task
/// manual verification (T02).
/// </summary>
public sealed class MockAIService : IAIService
{
    private static readonly string[] FallbackSuggestions =
    {
        "Tell me more",
        "Give an example",
        "Why?",
        "Summarize this",
    };

    private readonly TimeSpan _simulatedLatency;

    /// <param name="simulatedLatencyMs">
    /// Latency in milliseconds to simulate before responding. Defaults to
    /// 400ms so the UI can demonstrate the busy state without feeling
    /// sluggish.
    /// </param>
    public MockAIService(int simulatedLatencyMs = 400)
    {
        _simulatedLatency = TimeSpan.FromMilliseconds(Math.Max(0, simulatedLatencyMs));
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

        // Stay responsive on the UI thread by awaiting the simulated
        // network round-trip on a background scheduler (NFR-8).
        if (_simulatedLatency > TimeSpan.Zero)
        {
            await Task.Delay(_simulatedLatency, ct).ConfigureAwait(false);
        }

        if (ct.IsCancellationRequested)
        {
            return null;
        }

        return new ContextSuggestionResponse
        {
            Answer = BuildAnswer(prompt, systemContext),
            Suggestions = new List<string>(FallbackSuggestions),
        };
    }

    private static string BuildAnswer(string prompt, string? systemContext)
    {
        var sb = new StringBuilder();
        sb.Append("(Mock) ");

        if (!string.IsNullOrWhiteSpace(systemContext))
        {
            sb.Append("Responding using agent behaviour. ");
        }

        sb.Append("You said: ");
        sb.Append(prompt.Trim());
        sb.Append('.');
        return sb.ToString();
    }
}
