namespace NutriLens.Services;

/// <summary>
/// Centralized Azure OpenAI configuration. Values are placeholders —
/// replace them with your own endpoint, deployment name, and API key
/// before running the app. The sample never commits a real secret.
/// </summary>
public static class AzureOpenAIConfig
{
    public const string BaseEndpoint = "https://your-openai-resource.openai.azure.com/";

    public const string DeploymentName = "your-DeploymentName";

    public const string ApiKey = "your-azure-openai-api-key";
}