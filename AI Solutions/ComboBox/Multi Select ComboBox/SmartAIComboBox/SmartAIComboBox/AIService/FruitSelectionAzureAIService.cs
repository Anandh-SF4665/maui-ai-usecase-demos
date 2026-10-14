using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SmartAIComboBox;

/// <summary>
/// Azure OpenAI fruit-selection service for the SmartAIComboBox sample.
/// Mirrors the SmartAIDatePicker.AzureOpenAIService pattern exactly:
/// no constructor logic, no IsCredentialValid probe, no SK abstractions.
/// Errors are surfaced as exceptions (caller publishes to its bound
/// <c>ErrorMessage</c>) — never as magic-prefixed strings.
/// </summary>
public sealed class FruitSelectionAzureAIService : IFruitSelectionAIService
{
    private const string BaseEndpoint =
        "Your Endpoint";

    private const string DeploymentName = "Your Deployment Name";

    private const string ApiKey =
        "Your API Key";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Sends the user prompt to the chat-completions endpoint and returns the
    /// raw completion text. Throws on transport / HTTP / parse failures so the
    /// caller can surface them through its own error channel.
    /// </summary>
    public async Task<string> GetCompletionAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = DeploymentName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content =
                        "You are an AI fruit-selection assistant for a MultiSelect fruit menu. " +
                        "Choose fruits from the provided menu that match the user's natural-language request. " +
                        "Return ONLY the exact fruit Names from the menu, one per line. " +
                        "Do NOT include tags, numbers, dashes, bullets, explanations, or any preamble. " +
                        "If no fruit matches, return exactly the word: Empty"
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            max_completion_tokens = 800,
            reasoning_effort = "minimal"
        };

        var url = $"{BaseEndpoint}/chat/completions";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await Http.SendAsync(request, cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        Debug.WriteLine($"[AzureAI] Status: {(int)response.StatusCode}");
        Debug.WriteLine($"[AzureAI] Raw Response: {raw}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Azure OpenAI request failed with HTTP {(int)response.StatusCode} {response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        using var document = JsonDocument.Parse(raw);

        if (!document.RootElement.TryGetProperty("choices", out var choices)
            || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI response did not contain a 'choices' array. Raw response: {Truncate(raw, 4000)}");
        }

        var content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                $"Azure OpenAI returned empty content. Raw response: {Truncate(raw, 4000)}");
        }

        return content.Trim();
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
