using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIFileSearchTreeView
{
    public class SemanticSearchService : ISemanticSearchService
    {
        private const string DefaultEndpoint = "";
        private const string DefaultDeploymentName = "";
        private const string DefaultApiKey = "";

        private const string SystemPrompt =  """
You are an intelligent natural-language file search assistant.

Your goal is to identify and rank the most relevant files from the supplied TreeView records.

Search Rules

1. Compare the user's complete query against all supplied file records.

2. Determine relevance using all available metadata together:
   - File Name
   - Folder Path
   - Keywords
   - File Type
   - Created Date
   - Modified Date

3. Understand the user's overall intent.
   Do not perform simple keyword matching.

4. Use reasonable semantic relationships and synonyms when supported by the metadata.

   Examples:
   - wifi → wireless, access point, network connectivity
   - internet → broadband, connectivity, network
   - coverage → survey, test, assessment
   - invoice → bill, statement, billing record
   - security → firewall, policy, protection, compliance
   - design → architecture, topology, diagram

5. Consider the complete meaning of the query.
   A file is relevant only when its overall metadata supports the user's intent.

6. Do not return a file simply because it shares a single broad keyword.

7. When the query contains words such as:
   - recent
   - latest
   - newest
   - current

   prefer newer relevant files first.

8. Recency affects ranking only.
   It must not make unrelated files relevant.

9. Return all genuinely relevant files when multiple files help satisfy the user's request.

10. Return fewer files when only a small number of files are strongly relevant.

11. Folder records provide context only.
    Never return folder paths as results.

12. Return only exact leaf-file paths that exist in the supplied records.

13. Order results from most relevant to least relevant.

Output Rules

14. Return one exact file path per line.

15. Do not return:
    - explanations
    - reasoning
    - confidence scores
    - markdown
    - bullets
    - numbering
    - folder names
    - invented paths

16. Return an empty response when no supplied file is relevant.
""";

        private AzureOpenAIClient? _client;
        private bool _isCredentialValid;
        private string _deploymentName = string.Empty;

        public bool IsCredentialValid => _isCredentialValid;

        public SemanticSearchService()
        {
            InitializeCredentials();
        }

        public SemanticSearchService(string endpoint, string deploymentName, string apiKey)
        {
            InitializeClient(endpoint, deploymentName, apiKey);
        }

        private void InitializeCredentials()
        {
            InitializeClient(
                Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? DefaultEndpoint,
                Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? DefaultDeploymentName,
                Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? DefaultApiKey);
        }

        private void InitializeClient(string? endpoint, string? deploymentName, string? apiKey)
        {
            _deploymentName = deploymentName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(_deploymentName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                !Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var endpointUri) ||
                (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps))
            {
                _client = null;
                _isCredentialValid = false;
                return;
            }

            try
            {
                _client = new AzureOpenAIClient(endpointUri, new ApiKeyCredential(apiKey));
                _isCredentialValid = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Azure OpenAI initialization failed: {ex.Message}");
                _client = null;
                _isCredentialValid = false;
            }
        }

        public async Task<List<string>> SearchFilesSemanticAsync(
            string query,
            List<FileMetadataContext> fileMetadata,
            CancellationToken cancellationToken = default)
        {
            if (!_isCredentialValid || _client == null || string.IsNullOrWhiteSpace(query) || fileMetadata.Count == 0)
                return new List<string>();

            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage(BuildUserPrompt(query, fileMetadata))
                };

                var response = await _client
                    .GetChatClient(_deploymentName)
                    .CompleteChatAsync(messages, cancellationToken: cancellationToken);

                var content = string.Join(
                    Environment.NewLine,
                    response.Value.Content
                        .Select(part => part.Text)
                        .Where(text => !string.IsNullOrWhiteSpace(text)));

                return ValidateReturnedPaths(ParseSearchResults(content), fileMetadata);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Semantic search failed: {ex.Message}");
                return new List<string>();
            }
        }

        private static string BuildUserPrompt(string query, IEnumerable<FileMetadataContext> fileMetadata)
        {
            var fileList = new StringBuilder();
            foreach (var file in fileMetadata)
            {
                var recordType = string.IsNullOrWhiteSpace(file.FileType) ? "File" : file.FileType;
                fileList.Append("- Path: ").Append(file.FullPath);
                fileList.Append(" | Name: ").Append(file.FileName);
                fileList.Append(" | Type: ").Append(recordType);
                fileList.Append(" | Created: ").Append(file.CreatedDate.ToString("yyyy-MM-dd"));
                fileList.Append(" | Modified: ").Append(file.ModifiedDate.ToString("yyyy-MM-dd"));

                if (file.Keywords?.Length > 0)
                    fileList.Append(" | Keywords: ").Append(string.Join(", ", file.Keywords));

                fileList.AppendLine();
            }

            return $"""
                User query:
                {query.Trim()}

                Files in the current TreeView:
                {fileList}
                """;
        }

        private static List<string> ParseSearchResults(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return new List<string>();

            return response
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim().TrimStart('-', '*').Trim())
                .Where(line => line.StartsWith('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> ValidateReturnedPaths(
            IEnumerable<string> returnedPaths,
            IEnumerable<FileMetadataContext> fileMetadata)
        {
            var canonicalPaths = fileMetadata
                .Where(file => !string.IsNullOrWhiteSpace(file.FullPath))
                .ToDictionary(file => NormalizePath(file.FullPath), file => file.FullPath, StringComparer.OrdinalIgnoreCase);

            return returnedPaths
                .Where(path => canonicalPaths.ContainsKey(NormalizePath(path)))
                .Select(path => canonicalPaths[NormalizePath(path)])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizePath(string path) =>
            path.Replace('\\', '/').Trim().ToLowerInvariant();
    }
}
