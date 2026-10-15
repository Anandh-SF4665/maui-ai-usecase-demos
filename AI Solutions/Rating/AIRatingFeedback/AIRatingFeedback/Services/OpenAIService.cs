using System.ClientModel;
using System.Globalization;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIRatingFeedback.Services;

public class OpenAIService
{
    private const string DefaultEndpoint = "";
    private const string DefaultDeploymentName = "";
    private const string DefaultApiKey = "";

    private AzureOpenAIClient? _client;
    private readonly string _deploymentName;
    private bool _isClientConfigured;

    // Parameterless constructor — uses the built-in default Azure OpenAI credentials.
    // MauiProgram registers OpenAIService with this constructor via dependency injection.
    public OpenAIService()
    {
        _deploymentName = DefaultDeploymentName;
        _isClientConfigured = TryCreateClient(DefaultEndpoint, DefaultDeploymentName, DefaultApiKey, out _client);
    }

    // Constructor that accepts custom credentials (e.g. for testing or user-supplied values).
    public OpenAIService(string endpoint, string deploymentName, string apiKey)
    {
        _deploymentName = deploymentName;
        _isClientConfigured = TryCreateClient(endpoint, deploymentName, apiKey, out _client);
    }

    /// <summary>
    /// Validates the endpoint/deployment/key strings and constructs an
    /// <see cref="AzureOpenAIClient"/>. Returns <c>false</c> (instead of throwing)
    /// when the inputs are missing or malformed.
    /// </summary>
    private static bool TryCreateClient(
        string endpoint,
        string deploymentName,
        string apiKey,
        out AzureOpenAIClient? client)
    {
        client = null;

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(deploymentName))
        {
            return false;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        try
        {
            client = new AzureOpenAIClient(uri, new ApiKeyCredential(apiKey));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> GetSuggestedRatingAsync(string feedback, CancellationToken cancellationToken = default)
    {
        if (!_isClientConfigured || _client is null)
        {
            return 0;
        }

        List<ChatMessage> messages =
        [
            new SystemChatMessage(
                """
                You are an AI assistant that analyzes customer support feedback and determines an appropriate satisfaction rating.

                Your task is to understand the customer's overall experience and assign a decimal rating from 1.0 to 5.0 based on the sentiment expressed in the feedback.

                Consider factors such as:
                    - Issue resolution effectiveness
                    - Response time
                    - Professionalism of the support agent
                    - Helpfulness of the support provided
                    - Customer satisfaction or frustration
                    - Ease of resolving the issue
                    - Whether the customer would likely have a positive or negative impression of the support experience

                Rating Guidelines:
                    0.0: Not enough information, incomplete feedback, unfinished sentences, random text, gibberish, unrelated content, or feedback that does not clearly describe a customer support experience.
                    0.1 - 1.0: The customer is extremely dissatisfied. Feedback indicates a very poor experience, unresolved issues, repeated problems, strong frustration, or disappointment.
                    1.1 - 2.0: The customer is dissatisfied. Feedback indicates significant delays, inadequate assistance, poor communication, or unresolved concerns.
                    2.1 - 3.0: The experience is mixed or neutral. The feedback contains both positive and negative aspects, or the customer's satisfaction level is unclear.
                    3.1 - 4.0: The customer is generally satisfied. The issue was resolved, but there may be minor concerns such as slightly delayed responses or small frustrations.
                    4.1 - 5.0: The customer is highly satisfied. Feedback indicates excellent service, quick resolution, professionalism, clear communication, or an overall positive experience.

                Return ONLY a single number: 0.0, 1, 1.4, 2, 2.7, 3, 3.5, 4, 4.6, 5.0,
                Do not return explanations, labels, punctuation, or additional text.
                """
            ),

            new UserChatMessage(feedback)
        ];

        try
        {
            ChatCompletion response = await _client
                    .GetChatClient(_deploymentName)
                    .CompleteChatAsync(messages, cancellationToken: cancellationToken);

            string result = response.Content[0].Text.Trim();
            if (double.TryParse(result, NumberStyles.Float | NumberStyles.AllowThousands,
                                CultureInfo.InvariantCulture, out double rating))
            {
                return Math.Clamp(rating, 0.0, 5.0);
            }

            return 0.0;
        }
        catch (OperationCanceledException)
        {
            // Debounce / superseded by a newer keystroke — not an error.
            throw;
        }
        catch (ClientResultException ex)
        {
            // Surface the real Azure error: status code + message + inner exception.
            throw new InvalidOperationException(
                $"Azure OpenAI request failed (HTTP {(int)ex.Status} {ex.Status}). " +
                $"{ex.Message}", ex);
        }
    }
}