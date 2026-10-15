using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.TreeView;
using System.Collections.ObjectModel;

namespace AIFileSearchTreeView
{
    public partial class MainPage : ContentPage
    {
        private SalesFileViewModel? _viewModel;
        private CancellationTokenSource? _searchCancellationTokenSource;
        private int _searchRequestVersion;

        public MainPage()
        {
            InitializeComponent();

            // Inject ViewModel via ServiceHelper (from DI container)
            try
            {
                _viewModel = ServiceHelper.Services.GetRequiredService<SalesFileViewModel>();
                this.BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                throw;
            }

            // Attach search event with debouncing
            if (searchEntry != null)
            {
                searchEntry.TextChanged += OnSearchEntryTextChanged;
            }
        }

        /// <summary>
        /// Handle search entry text changes with AI semantic search
        /// </summary>
        private async void OnSearchEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_viewModel == null) return;

            string searchText = e.NewTextValue?.Trim() ?? "";
            _searchRequestVersion++;
            var requestVersion = _searchRequestVersion;

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // ensure loading indicator hidden on empty
                _viewModel.IsSearching = false;
                SetSearchBusy(false);
                try
                {
                    await _viewModel.PerformSemanticSearch(searchText, token);
                }
                catch (OperationCanceledException)
                {
                }

                if (requestVersion == _searchRequestVersion)
                {
                    CollapseAllNodes();
                }

                return;
            }

            try
            {
                // show loading indicator immediately while user types / during debounce
                _viewModel.IsSearching = true;
                SetSearchBusy(true);
                await Task.Yield();
                await Task.Delay(350, token);
                if (requestVersion != _searchRequestVersion)
                    return;

                var result = await _viewModel.PerformSemanticSearch(searchText, token);
                if (requestVersion != _searchRequestVersion)
                    return;

                ExpandAllMatchedNodes(result);
            }
            catch (OperationCanceledException)
            {
                // Only the current request may change the loading state.
                if (requestVersion == _searchRequestVersion)
                {
                    _viewModel.IsSearching = false;
                    SetSearchBusy(false);
                }
            }
            finally
            {
                if (requestVersion == _searchRequestVersion)
                {
                    _viewModel.IsSearching = false;
                    SetSearchBusy(false);
                }
            }
        }

        private void SetSearchBusy(bool isBusy)
        {
            if (searchBusyIndicator == null)
                return;

            searchBusyIndicator.IsVisible = isBusy;
            searchBusyIndicator.IsRunning = isBusy;
        }

        /// <summary>
        /// Apply the search result to display ONLY matched files in the TreeView.
        /// The TreeView's ItemsSource is swapped to the filtered tree that contains
        /// only matched files inside their ancestor folders; all non-matched files and
        /// folders are completely hidden (not just collapsed).
        /// </summary>
        private void ExpandAllMatchedNodes(SearchResult result)
        {
            if (treeView == null || _viewModel?.SalesFileNodes == null)
                return;

            try
            {
                // ExpandAndHighlightMatches marks nodes AND builds FilteredSalesFileNodes
                // (the filtered tree containing only matched files + ancestor folders).
                _viewModel.ExpandAndHighlightMatches(result);

                // Swap the TreeView to the filtered tree so ONLY matched files and their
                // ancestor folders are rendered. Non-matched files are not displayed.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    treeView.ItemsSource = _viewModel.FilteredSalesFileNodes;
                    treeView.ExpandAll();
                });
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Restore the full tree view with all files and clear highlights.
        /// The TreeView ItemsSource is swapped back to the full tree so that all
        /// files/folders become visible again after a search is cleared.
        /// </summary>
        private void CollapseAllNodes()
        {
            if (treeView == null || _viewModel?.SalesFileNodes == null)
                return;

            try
            {
                // Clear all highlights and match markers
                MarkAllNodesForCollapse(_viewModel.SalesFileNodes.OfType<SalesFileNode>());

                // Swap the TreeView back to the FULL tree (all files/folders visible).
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    treeView.ItemsSource = _viewModel.SalesFileNodes;
                });

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Mark all nodes with IsExpanded = false
        /// </summary>
        private void MarkAllNodesForCollapse(IEnumerable<SalesFileNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.IsExpanded = false;
                node.IsMatched = false;
                node.IsHighlighted = false;

                // Recursively collapse children
                if (node.SubFiles?.Count > 0)
                {
                    MarkAllNodesForCollapse(node.SubFiles.OfType<SalesFileNode>());
                }
            }
        }

        /// <summary>
        /// Clear search and highlights
        /// </summary>
        private void OnClearSearch(object sender, EventArgs e)
        {
            if (_viewModel == null) return;

            searchEntry.Text = "";
            _viewModel.MatchCountLabel = "";
            _viewModel.MatchCount = 0;
            _viewModel.IsSearching = false;

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();

            CollapseAllNodes();
        }
    }
}
