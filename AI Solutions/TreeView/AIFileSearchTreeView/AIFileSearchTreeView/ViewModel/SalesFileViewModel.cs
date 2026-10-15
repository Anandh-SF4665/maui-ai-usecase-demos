using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIFileSearchTreeView
{
    /// <summary>
    /// ViewModel for AI-powered sales file search with semantic understanding.
    /// Integrates Azure OpenAI for intelligent hierarchical filtering.
    /// </summary>
    public class SalesFileViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SalesFileNode> salesFileNodes = new();
        private ObservableCollection<SalesFileNode> filteredSalesFileNodes = new();
        private int matchCount;
        private bool isSearching;
        private string matchCountLabel = string.Empty;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ISemanticSearchService _semanticSearchService;
        private readonly Stopwatch _searchStopwatch = new Stopwatch();

        public ObservableCollection<SalesFileNode> SalesFileNodes
        {
            get => salesFileNodes;
            set { salesFileNodes = value; RaisedOnPropertyChanged(nameof(SalesFileNodes)); }
        }

        /// <summary>
        /// Filtered tree containing ONLY matched files inside their ancestor folders.
        /// The TreeView binds here during a search so non-matched files/folders are
        /// completely hidden (not just collapsed). Reset to empty on clear, at which
        /// point the TreeView falls back to the full SalesFileNodes collection.
        /// </summary>
        public ObservableCollection<SalesFileNode> FilteredSalesFileNodes
        {
            get => filteredSalesFileNodes;
            set { filteredSalesFileNodes = value; RaisedOnPropertyChanged(nameof(FilteredSalesFileNodes)); }
        }

        public int MatchCount
        {
            get => matchCount;
            set { matchCount = value; RaisedOnPropertyChanged(nameof(MatchCount)); }
        }

        public bool IsSearching
        {
            get => isSearching;
            set { isSearching = value; RaisedOnPropertyChanged(nameof(IsSearching)); }
        }

        public string MatchCountLabel
        {
            get => matchCountLabel;
            set { matchCountLabel = value; RaisedOnPropertyChanged(nameof(MatchCountLabel)); }
        }

        public SalesFileViewModel(ISemanticSearchService semanticSearchService)
        {
            _semanticSearchService = semanticSearchService ?? throw new ArgumentNullException(nameof(semanticSearchService));
            InitializeSalesData();
        }

        private void InitializeSalesData()
        {
            SalesFileNodes = GenerateSalesFileStructure();
            MatchCountLabel = string.Empty;
        }

        private ObservableCollection<SalesFileNode> GenerateSalesFileStructure()
        {
            var root = new ObservableCollection<SalesFileNode>();

            // =========================================================
            // ROOT: Downloads (10 files - permanent network/WiFi/broadband documents)
            // Files here use TOPIC-BASED names only — no month or year suffixes — so
            // search queries like 'wifi invoice' or 'wifi coverage report' resolve to
            // a single, unambiguous file rather than collapsing to whatever happened
            // to be downloaded last month. Recency is communicated via CreatedDate.
            // =========================================================
            var downloadsNode = new SalesFileNode
            {
                ItemName = "Downloads",
                ImageIcon = "treeview_folder.png",
                SearchPath = "/Downloads",
                FileType = "Folder",
                Keywords = new[] { "downloads", "incoming", "bills", "statements", "invoices" }
            };

            var downloadsFiles = new ObservableCollection<SalesFileNode>
            {
                new SalesFileNode
                {
                    ItemName = "WiFi_Invoice.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/WiFi_Invoice.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "wifi", "wireless", "network", "invoice", "billing" },
                    CreatedDate = new DateTime(2026, 9, 15),
                    ModifiedDate = new DateTime(2026, 9, 15),
                    InvoiceAmount = 55.00m
                },
                new SalesFileNode
                {
                    ItemName = "Wireless_Invoice.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Wireless_Invoice.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "wireless", "wifi", "network", "invoice", "billing" },
                    CreatedDate = new DateTime(2026, 9, 10),
                    ModifiedDate = new DateTime(2026, 9, 10),
                    InvoiceAmount = 59.50m
                },
                new SalesFileNode
                {
                    ItemName = "Network_Invoice.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Network_Invoice.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "network", "internet", "connectivity", "invoice", "billing" },
                    CreatedDate = new DateTime(2026, 9, 5),
                    ModifiedDate = new DateTime(2026, 9, 5),
                    InvoiceAmount = 67.25m
                },
                new SalesFileNode
                {
                    ItemName = "Broadband_Invoice.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Broadband_Invoice.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "broadband", "network", "internet", "wifi", "connectivity", "invoice", "billing" },
                    CreatedDate = new DateTime(2026, 8, 28),
                    ModifiedDate = new DateTime(2026, 8, 28),
                    InvoiceAmount = 72.00m
                },
                new SalesFileNode
                {
                    ItemName = "Connectivity_Invoice.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Connectivity_Invoice.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "connectivity", "network", "internet", "invoice", "billing" },
                    CreatedDate = new DateTime(2026, 8, 20),
                    ModifiedDate = new DateTime(2026, 8, 20),
                    InvoiceAmount = 64.99m
                },
                new SalesFileNode
                {
                    ItemName = "Internet_Statement.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Internet_Statement.pdf",
                    FileType = "Invoice",
                    Keywords = new[] { "internet", "network", "connectivity", "statement", "billing" },
                    CreatedDate = new DateTime(2026, 8, 12),
                    ModifiedDate = new DateTime(2026, 8, 12),
                    InvoiceAmount = 58.75m
                },
                new SalesFileNode
                {
                    ItemName = "Broadband_Summary.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Broadband_Summary.pdf",
                    FileType = "Summary",
                    Keywords = new[] { "broadband", "network", "internet", "wifi", "summary" },
                    CreatedDate = new DateTime(2026, 8, 10),
                    ModifiedDate = new DateTime(2026, 8, 10)
                },
                new SalesFileNode
                {
                    ItemName = "Connection_Renewal_Record.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Downloads/Connection_Renewal_Record.pdf",
                    FileType = "Contract",
                    Keywords = new[] { "connection", "renewal", "network", "internet", "record", "contract" },
                    CreatedDate = new DateTime(2026, 8, 12),
                    ModifiedDate = new DateTime(2026, 8, 12)
                }
            };
            downloadsNode.SubFiles = new ObservableCollection<FileManager>(downloadsFiles.Cast<FileManager>());

            // =========================================================
            // ROOT: Documents (10 files - generic project docs)
            // =========================================================
            var documentsNode = new SalesFileNode
            {
                ItemName = "Documents",
                ImageIcon = "treeview_folder.png",
                SearchPath = "/Documents",
                FileType = "Folder",
                Keywords = new[] { "documents", "files", "records", "project" }
            };

            var documentsFiles = new ObservableCollection<SalesFileNode>
            {
                new SalesFileNode
                {
                    ItemName = "Project_Status_Report.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Documents/Project_Status_Report.docx",
                    FileType = "Report",
                    Keywords = new[] { "project", "status", "report", "progress" },
                    CreatedDate = new DateTime(2026, 1, 15),
                    ModifiedDate = new DateTime(2026, 1, 15)
                },
                new SalesFileNode
                {
                    ItemName = "Network_Design.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Documents/Network_Design.pdf",
                    FileType = "Document",
                    Keywords = new[] { "network", "design", "topology", "architecture", "internet", "wifi" },
                    CreatedDate = new DateTime(2026, 2, 2),
                    ModifiedDate = new DateTime(2026, 2, 2)
                },
                new SalesFileNode
                {
                    ItemName = "Meeting_Notes.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Documents/Meeting_Notes.docx",
                    FileType = "Document",
                    Keywords = new[] { "meeting", "notes", "minutes", "discussion" },
                    CreatedDate = new DateTime(2026, 2, 20),
                    ModifiedDate = new DateTime(2026, 2, 20)
                },
                new SalesFileNode
                {
                    ItemName = "User_Guide.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Documents/User_Guide.pdf",
                    FileType = "Document",
                    Keywords = new[] { "user", "guide", "manual", "documentation" },
                    CreatedDate = new DateTime(2026, 3, 10),
                    ModifiedDate = new DateTime(2026, 3, 10)
                },
                new SalesFileNode
                {
                    ItemName = "Budget_Plan.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Documents/Budget_Plan.xlsx",
                    FileType = "Document",
                    Keywords = new[] { "budget", "plan", "finance", "expense" },
                    CreatedDate = new DateTime(2026, 3, 25),
                    ModifiedDate = new DateTime(2026, 3, 25)
                },
                new SalesFileNode
                {
                    ItemName = "Training_Material.pptx",
                    ImageIcon = "treeview_ppt.png",
                    SearchPath = "/Documents/Training_Material.pptx",
                    FileType = "Document",
                    Keywords = new[] { "training", "material", "presentation", "learning" },
                    CreatedDate = new DateTime(2026, 4, 8),
                    ModifiedDate = new DateTime(2026, 4, 8)
                },
                new SalesFileNode
                {
                    ItemName = "Requirement_Document.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Documents/Requirement_Document.docx",
                    FileType = "Document",
                    Keywords = new[] { "requirement", "document", "specification", "scope" },
                    CreatedDate = new DateTime(2026, 4, 22),
                    ModifiedDate = new DateTime(2026, 4, 22)
                },
                new SalesFileNode
                {
                    ItemName = "Architecture_Diagram.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Documents/Architecture_Diagram.pdf",
                    FileType = "Document",
                    Keywords = new[] { "architecture", "diagram", "design", "system" },
                    CreatedDate = new DateTime(2026, 5, 12),
                    ModifiedDate = new DateTime(2026, 5, 12)
                },
                new SalesFileNode
                {
                    ItemName = "Policy_Document.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Documents/Policy_Document.pdf",
                    FileType = "Document",
                    Keywords = new[] { "policy", "document", "compliance", "rules" },
                    CreatedDate = new DateTime(2026, 6, 18),
                    ModifiedDate = new DateTime(2026, 6, 18)
                },
                new SalesFileNode
                {
                    ItemName = "Inventory_List.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Documents/Inventory_List.xlsx",
                    FileType = "Document",
                    Keywords = new[] { "inventory", "list", "assets", "tracking" },
                    CreatedDate = new DateTime(2026, 7, 1),
                    ModifiedDate = new DateTime(2026, 7, 1)
                }
            };
            documentsNode.SubFiles = new ObservableCollection<FileManager>(documentsFiles.Cast<FileManager>());

            // =========================================================
            // ROOT: Shared Files (10 files - team shared)
            // =========================================================
            var sharedFilesNode = new SalesFileNode
            {
                ItemName = "Shared Resources",
                ImageIcon = "treeview_folder.png",
                SearchPath = "/Shared Resources",
                FileType = "Folder",
                Keywords = new[] { "shared", "team", "collaboration", "common" }
            };

            var sharedFiles = new ObservableCollection<SalesFileNode>
            {
                new SalesFileNode
                {
                    ItemName = "Coverage_Test_Report.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Shared Resources/Coverage_Test_Report.pdf",
                    FileType = "Report",
                    Keywords = new[] { "coverage", "test", "report", "network", "wifi", "wireless" },
                    CreatedDate = new DateTime(2026, 1, 20),
                    ModifiedDate = new DateTime(2026, 1, 20)
                },
                new SalesFileNode
                {
                    ItemName = "Infrastructure_Notes.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Shared Resources/Infrastructure_Notes.docx",
                    FileType = "Document",
                    Keywords = new[] { "infrastructure", "notes", "network", "system" },
                    CreatedDate = new DateTime(2026, 2, 12),
                    ModifiedDate = new DateTime(2026, 2, 12)
                },
                new SalesFileNode
                {
                    ItemName = "Network_Upgrade_Plan.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Shared Resources/Network_Upgrade_Plan.xlsx",
                    FileType = "Document",
                    Keywords = new[] { "network", "upgrade", "plan", "internet", "wifi", "connectivity" },
                    CreatedDate = new DateTime(2026, 3, 5),
                    ModifiedDate = new DateTime(2026, 3, 5)
                },
                new SalesFileNode
                {
                    ItemName = "Access_Point_List.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Shared Resources/Access_Point_List.xlsx",
                    FileType = "Document",
                    Keywords = new[] { "access point", "list", "wifi", "wireless", "network" },
                    CreatedDate = new DateTime(2026, 3, 28),
                    ModifiedDate = new DateTime(2026, 3, 28)
                },
                new SalesFileNode
                {
                    ItemName = "Firewall_Settings.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Shared Resources/Firewall_Settings.pdf",
                    FileType = "Document",
                    Keywords = new[] { "firewall", "settings", "security", "network", "configuration" },
                    CreatedDate = new DateTime(2026, 4, 14),
                    ModifiedDate = new DateTime(2026, 4, 14)
                },
                new SalesFileNode
                {
                    ItemName = "Implementation_Plan.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Shared Resources/Implementation_Plan.docx",
                    FileType = "Document",
                    Keywords = new[] { "implementation", "plan", "rollout", "strategy" },
                    CreatedDate = new DateTime(2026, 5, 2),
                    ModifiedDate = new DateTime(2026, 5, 2)
                },
                new SalesFileNode
                {
                    ItemName = "Wireless_Survey_Report.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Shared Resources/Wireless_Survey_Report.pdf",
                    FileType = "Report",
                    Keywords = new[] { "wireless", "wifi", "network", "survey", "report" },
                    CreatedDate = new DateTime(2026, 5, 20),
                    ModifiedDate = new DateTime(2026, 5, 20)
                },
                new SalesFileNode
                {
                    ItemName = "Operations_Guide.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Shared Resources/Operations_Guide.docx",
                    FileType = "Document",
                    Keywords = new[] { "operations", "guide", "process", "procedure" },
                    CreatedDate = new DateTime(2026, 6, 8),
                    ModifiedDate = new DateTime(2026, 6, 8)
                },
                new SalesFileNode
                {
                    ItemName = "Performance_Report.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Shared Resources/Performance_Report.xlsx",
                    FileType = "Report",
                    Keywords = new[] { "performance", "report", "metrics", "kpi" },
                    CreatedDate = new DateTime(2026, 7, 3),
                    ModifiedDate = new DateTime(2026, 7, 3)
                },
                new SalesFileNode
                {
                    ItemName = "Technical_Document.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Shared Resources/Technical_Document.pdf",
                    FileType = "Document",
                    Keywords = new[] { "technical", "document", "specification", "engineering" },
                    CreatedDate = new DateTime(2026, 7, 25),
                    ModifiedDate = new DateTime(2026, 7, 25)
                }
            };
            sharedFilesNode.SubFiles = new ObservableCollection<FileManager>(sharedFiles.Cast<FileManager>());

            // =========================================================
            // ROOT: Archive (8 files - uncategorized)
            // =========================================================
            var archiveNode = new SalesFileNode
            {
                ItemName = "Archive",
                ImageIcon = "treeview_folder.png",
                SearchPath = "/Archive",
                FileType = "Folder",
                Keywords = new[] { "archive", "backup", "old", "legacy" }
            };

            var miscellaneousFiles = new ObservableCollection<SalesFileNode>
            {
                new SalesFileNode
                {
                    ItemName = "Random_Notes.txt",
                    ImageIcon = "treeview_text.png",
                    SearchPath = "/Archive/Random_Notes.txt",
                    FileType = "Document",
                    Keywords = new[] { "random", "notes", "text", "scratch" },
                    CreatedDate = new DateTime(2026, 1, 10),
                    ModifiedDate = new DateTime(2026, 1, 10)
                },
                new SalesFileNode
                {
                    ItemName = "Old_Report.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Archive/Old_Report.pdf",
                    FileType = "Report",
                    Keywords = new[] { "old", "report", "archive", "legacy" },
                    CreatedDate = new DateTime(2025, 12, 5),
                    ModifiedDate = new DateTime(2025, 12, 5)
                },
                new SalesFileNode
                {
                    ItemName = "Draft_Document.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Archive/Draft_Document.docx",
                    FileType = "Document",
                    Keywords = new[] { "draft", "document", "work in progress", "wip" },
                    CreatedDate = new DateTime(2026, 3, 18),
                    ModifiedDate = new DateTime(2026, 3, 18)
                },
                new SalesFileNode
                {
                    ItemName = "Temp_File.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Archive/Temp_File.pdf",
                    FileType = "Document",
                    Keywords = new[] { "temp", "temporary", "file" },
                    CreatedDate = new DateTime(2026, 4, 30),
                    ModifiedDate = new DateTime(2026, 4, 30)
                },
                new SalesFileNode
                {
                    ItemName = "Equipment_Details.xlsx",
                    ImageIcon = "treeview_excel.png",
                    SearchPath = "/Archive/Equipment_Details.xlsx",
                    FileType = "Document",
                    Keywords = new[] { "equipment", "details", "hardware", "specs" },
                    CreatedDate = new DateTime(2026, 5, 22),
                    ModifiedDate = new DateTime(2026, 5, 22)
                },
                new SalesFileNode
                {
                    ItemName = "Reference_Document.pdf",
                    ImageIcon = "treeview_pdf.png",
                    SearchPath = "/Archive/Reference_Document.pdf",
                    FileType = "Document",
                    Keywords = new[] { "reference", "document", "resource" },
                    CreatedDate = new DateTime(2026, 6, 14),
                    ModifiedDate = new DateTime(2026, 6, 14)
                },
                new SalesFileNode
                {
                    ItemName = "Test_Data.csv",
                    ImageIcon = "treeview_csv.png",
                    SearchPath = "/Archive/Test_Data.csv",
                    FileType = "Document",
                    Keywords = new[] { "test", "data", "sample", "csv" },
                    CreatedDate = new DateTime(2026, 7, 9),
                    ModifiedDate = new DateTime(2026, 7, 9)
                },
                new SalesFileNode
                {
                    ItemName = "General_Notes.docx",
                    ImageIcon = "treeview_word.png",
                    SearchPath = "/Archive/General_Notes.docx",
                    FileType = "Document",
                    Keywords = new[] { "general", "notes", "miscellaneous" },
                    CreatedDate = new DateTime(2026, 8, 1),
                    ModifiedDate = new DateTime(2026, 8, 1)
                }
            };
            archiveNode.SubFiles = new ObservableCollection<FileManager>(miscellaneousFiles.Cast<FileManager>());

            // Add primary roots (in order: Downloads, Documents, Shared Files, Miscellaneous)
            root.Add(downloadsNode);
            root.Add(documentsNode);
            root.Add(sharedFilesNode);
            root.Add(archiveNode);

            return root;
        }

        public async Task<SearchResult> PerformSemanticSearch(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                MatchCountLabel = "";
                MatchCount = 0;
                RestoreFullTree();
                return new SearchResult { SearchQuery = query, SearchExecutedAt = DateTime.Now };
            }

            cancellationToken.ThrowIfCancellationRequested();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IsSearching = true;
            _searchStopwatch.Restart();

            try
            {
                var result = new SearchResult
                {
                    SearchQuery = query,
                    SearchExecutedAt = DateTime.Now
                };

                var fileMetadata = BuildFileMetadataList();

                if (fileMetadata.Count == 0)
                {
                    MatchCountLabel = "No data to search.";
                    return result;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var matchedPaths = await _semanticSearchService.SearchFilesSemanticAsync(
                    query,
                    fileMetadata,
                    _cancellationTokenSource.Token
                );

                cancellationToken.ThrowIfCancellationRequested();

                result.Matches = matchedPaths.Select(path => new SearchResult.MatchedNode
                {
                    FilePath = path,
                    FileName = ExtractFileName(path),
                    MatchReasons = new[] { "Matched by AI semantic search" },
                    ConfidenceScore = 0.95
                }).ToList();

                _searchStopwatch.Stop();
                result.ExecutionTimeMs = _searchStopwatch.ElapsedMilliseconds;

                MatchCount = result.TotalMatches;
                MatchCountLabel = MatchCount > 0
                    ? MatchCount == 1
                        ? "✓ Found match in 1 file"
                        : $"✓ Found {MatchCount} matches in {MatchCount} files"
                    : "No matches found.";

                return result;
            }
            catch (OperationCanceledException)
            {
                MatchCountLabel = string.Empty;
                ClearAllHighlights();
                return new SearchResult { SearchQuery = query, SearchExecutedAt = DateTime.Now };
            }
            catch (Exception ex)
            {
                MatchCountLabel = $"Search error: {ex.Message}";
                ClearAllHighlights();
                return new SearchResult { SearchQuery = query, SearchExecutedAt = DateTime.Now };
            }
            finally
            {
                IsSearching = false;
            }
        }

        public void ExpandAndHighlightMatches(SearchResult result)
        {
            ClearAllHighlights();

            if (result?.Matches == null || result.Matches.Count == 0)
            {
                FilteredSalesFileNodes = new ObservableCollection<SalesFileNode>();
                return;
            }

            var matchedPaths = result.Matches
                .Where(match => !string.IsNullOrWhiteSpace(match.FilePath))
                .Select(match => match.FilePath.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            void MarkNodesRecursive(ObservableCollection<FileManager> nodes)
            {
                if (nodes == null || nodes.Count == 0)
                    return;

                foreach (var node in nodes.OfType<SalesFileNode>().ToList())
                {
                    if (string.IsNullOrWhiteSpace(node.SearchPath))
                    {
                        continue;
                    }

                    var isExactMatch = matchedPaths.Contains(node.SearchPath);
                    var hasMatchingDescendant = matchedPaths.Any(path =>
                        path.StartsWith(node.SearchPath + "/", StringComparison.OrdinalIgnoreCase));

                    if (isExactMatch)
                    {
                        node.IsMatched = true;
                        node.IsHighlighted = true;
                        node.IsExpanded = true;

                        var matchingResult = result.Matches.FirstOrDefault(match =>
                            match.FilePath.Equals(node.SearchPath, StringComparison.OrdinalIgnoreCase));

                        if (matchingResult != null)
                        {
                            node.MatchConfidenceScore = matchingResult.ConfidenceScore;
                        }
                    }
                    else if (hasMatchingDescendant)
                    {
                        node.IsHighlighted = true;
                        node.IsExpanded = true;
                    }
                    else
                    {
                        node.IsExpanded = false;
                    }

                    if (node.SubFiles?.Count > 0)
                    {
                        MarkNodesRecursive(node.SubFiles);
                    }
                }
            }

            MarkNodesRecursive(new ObservableCollection<FileManager>(SalesFileNodes.Cast<FileManager>()));

            FilteredSalesFileNodes = BuildFilteredTree(SalesFileNodes, matchedPaths);
        }

        private ObservableCollection<SalesFileNode> BuildFilteredTree(
            ObservableCollection<SalesFileNode> sourceNodes,
            HashSet<string> matchedPaths)
        {
            var filteredRoot = new ObservableCollection<SalesFileNode>();

            foreach (var node in sourceNodes)
            {
                if (string.IsNullOrWhiteSpace(node.SearchPath))
                    continue;

                var isExactMatch = matchedPaths.Contains(node.SearchPath);
                var hasMatchingDescendant = matchedPaths.Any(path =>
                    path.StartsWith(node.SearchPath + "/", StringComparison.OrdinalIgnoreCase));

                if (isExactMatch)
                {
                    filteredRoot.Add(CloneNode(node));
                }
                else if (hasMatchingDescendant)
                {
                    var filteredChildren = new ObservableCollection<FileManager>();
                    foreach (var child in node.SubFiles?.OfType<SalesFileNode>() ?? Enumerable.Empty<SalesFileNode>())
                    {
                        var collected = BuildFilteredNode(child, matchedPaths);
                        if (collected != null)
                            filteredChildren.Add(collected);
                    }

                    if (filteredChildren.Count > 0)
                    {
                        var folder = CloneNode(node);
                        folder.SubFiles = filteredChildren;
                        folder.IsExpanded = true;
                        folder.IsHighlighted = true;
                        filteredRoot.Add(folder);
                    }
                }
            }

            return filteredRoot;
        }

        private SalesFileNode? BuildFilteredNode(
            SalesFileNode node,
            HashSet<string> matchedPaths)
        {
            if (string.IsNullOrWhiteSpace(node.SearchPath))
                return null;

            var isExactMatch = matchedPaths.Contains(node.SearchPath);
            var hasMatchingDescendant = matchedPaths.Any(path =>
                path.StartsWith(node.SearchPath + "/", StringComparison.OrdinalIgnoreCase));

            if (isExactMatch)
                return CloneNode(node);

            if (hasMatchingDescendant)
            {
                var filteredChildren = new ObservableCollection<FileManager>();
                foreach (var child in node.SubFiles?.OfType<SalesFileNode>() ?? Enumerable.Empty<SalesFileNode>())
                {
                    var collected = BuildFilteredNode(child, matchedPaths);
                    if (collected != null)
                        filteredChildren.Add(collected);
                }

                if (filteredChildren.Count == 0)
                    return null;

                var folder = CloneNode(node);
                folder.SubFiles = filteredChildren;
                folder.IsExpanded = true;
                folder.IsHighlighted = true;
                return folder;
            }

            return null;
        }

        private static SalesFileNode CloneNode(SalesFileNode node)
        {
            return new SalesFileNode
            {
                ItemName = node.ItemName,
                ImageIcon = node.ImageIcon,
                SearchPath = node.SearchPath,
                FileType = node.FileType,
                Keywords = node.Keywords?.ToArray() ?? Array.Empty<string>(),
                ClientNames = node.ClientNames?.ToArray() ?? Array.Empty<string>(),
                Quarter = node.Quarter,
                InvoiceAmount = node.InvoiceAmount,
                CreatedDate = node.CreatedDate,
                ModifiedDate = node.ModifiedDate,
                IsMatched = node.IsMatched,
                IsHighlighted = node.IsHighlighted,
                IsExpanded = node.IsExpanded,
                MatchConfidenceScore = node.MatchConfidenceScore,
                SubFiles = new ObservableCollection<FileManager>()
            };
        }

        /// <summary>
        /// Restores the full tree when search is cleared
        /// </summary>
        private void RestoreFullTree()
        {
            void RestoreRecursive(ObservableCollection<FileManager> nodes)
            {
                if (nodes == null)
                    return;

                foreach (var node in nodes.OfType<SalesFileNode>())
                {
                    node.IsMatched = false;
                    node.IsHighlighted = false;
                    node.IsExpanded = false;
                    node.MatchConfidenceScore = 0;

                    if (node.SubFiles?.Count > 0)
                        RestoreRecursive(node.SubFiles);
                }
            }

            RestoreRecursive(new ObservableCollection<FileManager>(SalesFileNodes.Cast<FileManager>()));

            // Clear the filtered collection so the TreeView falls back to the full tree.
            FilteredSalesFileNodes = new ObservableCollection<SalesFileNode>();
        }

        private void ClearAllHighlights()
        {
            void ClearRecursive(ObservableCollection<FileManager> nodes)
            {
                if (nodes == null)
                    return;

                foreach (var node in nodes.OfType<SalesFileNode>())
                {
                    node.IsMatched = false;
                    node.IsHighlighted = false;
                    node.IsExpanded = false;
                    node.MatchConfidenceScore = 0;

                    if (node.SubFiles?.Count > 0)
                        ClearRecursive(node.SubFiles);
                }
            }

            ClearRecursive(new ObservableCollection<FileManager>(SalesFileNodes.Cast<FileManager>()));
        }

        private List<FileMetadataContext> BuildFileMetadataList()
        {
            var metadata = new List<FileMetadataContext>();
            void CollectMetadata(ObservableCollection<FileManager> nodes)
            {
                if (nodes == null)
                    return;

                foreach (var node in nodes.OfType<SalesFileNode>())
                {
                    if (node.FileType != "Folder")
                    {
                        metadata.Add(new FileMetadataContext
                        {
                            FullPath = node.SearchPath,
                            FileName = node.ItemName,
                            CreatedDate = node.CreatedDate,
                            ModifiedDate = node.ModifiedDate,
                            FileType = node.FileType ?? string.Empty,
                            Keywords = node.Keywords ?? Array.Empty<string>(),
                            RelatedClients = node.ClientNames ?? Array.Empty<string>(),
                            Quarter = node.Quarter ?? string.Empty,
                            Amount = node.InvoiceAmount
                        });
                    }

                    if (node.SubFiles?.Count > 0)
                        CollectMetadata(node.SubFiles);
                }
            }

            CollectMetadata(new ObservableCollection<FileManager>(SalesFileNodes.Cast<FileManager>()));
            
            return metadata;
        }

        private string ExtractFileName(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            var segments = fullPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments.Last() : fullPath;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisedOnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
