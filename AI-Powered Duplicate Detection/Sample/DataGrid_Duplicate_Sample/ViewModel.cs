using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataGrid_Duplicate_Sample.Analysis;
using DataGrid_Duplicate_Sample.Models;
using DataGrid_Duplicate_Sample.Services;

namespace DataGrid_Duplicate_Sample;

public partial class ViewModel : ObservableObject
{
    private readonly IDuplicateAnalyzer _analyzer;
    private readonly PairCandidateGenerator _generator = new();
    private CancellationTokenSource? cancelToken;

    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<DuplicateResult> Duplicates { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Ready.";


    public ViewModel(IDuplicateAnalyzer analyzer)
    {
        _analyzer = analyzer;
        foreach (var c in CustomerSeed.Build())
            Customers.Add(c);
    }

    [RelayCommand]
    private async Task DeleteRow(Customer? customer)
    {
        if (customer is null) return;

        // Confirm with the user before deleting.
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            var confirmed = await page.DisplayAlertAsync(
                                "Delete row",
                                $"Are you sure you want to delete the row \"{customer.Name}\"?",
                                "Delete",
                                "Cancel");

            if (!confirmed)
                return;
        }

        var index = Customers.IndexOf(customer);
        if (index < 0) 
            return;

        var deletedSignals = BuildDuplicateSignals(customer);
        Customers.RemoveAt(index);
        ClearOrphanedHighlightsAfterDelete(deletedSignals);
        StatusMessage = $"Deleted \"{customer.Name}\".";
    }

    [RelayCommand]
    private async Task IdentifyDuplicatesAsync()
    {
        if (IsBusy)
            return;

        cancelToken?.Cancel();
        var ct = new CancellationTokenSource();
        cancelToken = ct;

        try
        {
            IsBusy = true;
            StatusMessage = "Building candidate pairs…";
            var pairs = _generator.BuildPairs(Customers);
            StatusMessage = $"Sending candidate pair(s) to Azure OpenAI…";

            // Reset everyone to "Unique" before a full scan so stale highlights don't linger.
            foreach (var c in Customers)
            {
                c.RowHighlight = RowHighlight.None;
                c.Match = "—";
                c.Status = "Unique";
                c.IsEditing = false;
            }

            var results = await _analyzer.AnalyzeAsync(pairs, ct.Token);
            results = MergeIdenticalAddressResults(results, Customers);

            ApplyResults(results, source: HighlightSource.Scan);
            StatusMessage = $"Done. {results.Count(r => r.Classification == DuplicateClassification.LikelyDuplicate)} likely · "
                          + $"{results.Count(r => r.Classification == DuplicateClassification.PossibleDuplicate)} possible.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Triggered by SfDataGrid's CurrentCellEndEdit event after a user
    /// finishes editing a cell. Runs live duplicate detection for the
    /// affected row. Results show up in the row's Match / Status columns
    /// and the row is tinted with the live-edit (light-blue) highlight.
    /// </summary>
    public async Task OnCellCommittedAsync(Customer edited, CancellationToken ct)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = $"Live check for {edited.Id}…";

            // While we're checking, the row is in "Editing" state.
            edited.RowHighlight = RowHighlight.PossibleDuplicate;
            edited.Status = "Editing";
            edited.IsEditing = true;

            var others = Customers.Where(c => c.Id != edited.Id).ToList();
            var pairs = _generator.BuildPairs(new[] { edited }.Concat(others))
                .Where(p => p.A.Id == edited.Id || p.B.Id == edited.Id)
                .ToList();

            if (pairs.Count == 0)
            {
                StatusMessage = $"No candidate matches for {edited.Id}.";
                edited.Status = "Unique";
                edited.Match = "—";
                edited.IsEditing = false;
                edited.RowHighlight = RowHighlight.None;
                return;
            }

            var results = await _analyzer.AnalyzeAsync(pairs, ct);
            var all = new[] { edited }.Concat(others).ToList();
            var addressOnlyResults = BuildIdenticalAddressResults(all, all);
            foreach (var extra in addressOnlyResults)
            {
                if (results.Any(r =>
                    (r.RecordAId == extra.RecordAId && r.RecordBId == extra.RecordBId) ||
                    (r.RecordAId == extra.RecordBId && r.RecordBId == extra.RecordAId)))
                    continue;
                if (extra.RecordAId != edited.Id && extra.RecordBId != edited.Id) continue;
                results.Add(extra);
            }

            ApplyResults(results, focusRowId: edited.Id, source: HighlightSource.LiveEdit);
            StatusMessage = $"Live check complete for {edited.Id} — {results.Count} candidate pair(s) evaluated.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Live check error: " + ex.Message;
            edited.Status = "Unique";
            edited.Match = "—";
            edited.IsEditing = false;
            edited.RowHighlight = RowHighlight.None;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private enum HighlightSource { LiveEdit, Scan }

    private void ApplyResults(List<DuplicateResult> results, string? focusRowId = null, HighlightSource source = HighlightSource.Scan)
    {
        // 1. Clear stale highlights / match / status from rows that aren't affected by the new results.
        var affectedIds = new HashSet<string>();
        foreach (var r in results)
        {
            if (r.Classification == DuplicateClassification.NotDuplicate) continue;
            affectedIds.Add(r.RecordAId);
            affectedIds.Add(r.RecordBId);
        }

        foreach (var c in Customers)
        {
            if (focusRowId is not null && c.Id != focusRowId) continue;
            if (!affectedIds.Contains(c.Id))
            {
                c.RowHighlight = RowHighlight.None;
                c.Match = "—";
                c.Status = "Unique";
                c.IsEditing = false;
            }
        }

        // 2. For each row, take the strongest verdict it appears in.
        var bestById = new Dictionary<string, (DuplicateResult result, RowHighlight highlight)>();
        foreach (var r in results)
        {
            if (r.Classification == DuplicateClassification.NotDuplicate) continue;

            var tier = (int)r.RowHighlight;
            foreach (var id in new[] { r.RecordAId, r.RecordBId })
            {
                if (focusRowId is not null && id != focusRowId) continue; // live check: only the edited row

                if (!bestById.TryGetValue(id, out var existing) || tier > (int)existing.highlight)
                    bestById[id] = (r, r.RowHighlight);
            }
        }

        // 3. Paint rows.
        foreach (var c in Customers)
        {
            if (focusRowId is not null && c.Id != focusRowId) continue;

            if (bestById.TryGetValue(c.Id, out var best))
            {
                c.Match = $"{best.result.Confidence}%";
                c.Status = best.result.Classification switch
                {
                    DuplicateClassification.LikelyDuplicate   => "Likely duplicate",
                    DuplicateClassification.PossibleDuplicate => "Possible duplicate",
                    _ => "Unique"
                };

                c.IsEditing = false;
                c.RowHighlight = source == HighlightSource.LiveEdit
                    ? RowHighlight.PossibleDuplicate
                    : best.highlight;
            }
            else
            {
                c.Match = "—";
                c.Status = "Unique";
                c.IsEditing = false;
                c.RowHighlight = RowHighlight.None;
            }
        }

        // 4. Reset and refill the duplicates review list.
        Duplicates.Clear();
        var toShow = focusRowId is null
            ? results
            : results.Where(r => r.RecordAId == focusRowId || r.RecordBId == focusRowId);
        foreach (var r in toShow) Duplicates.Add(r);
    }

    /// <summary>
    /// Returns a new list of DuplicateResults containing the AI's existing
    /// results, plus a PossibleDuplicate (75% confidence) for every pair of
    /// customers that share an identical normalized address. If a same-address
    /// pair was already classified LikelyDuplicate by the AI, the AI's
    /// verdict is kept (we never downgrade). The AI never gets to overwrite a
    /// real duplicate with a weaker verdict.
    /// </summary>
    private static List<DuplicateResult> MergeIdenticalAddressResults(List<DuplicateResult> existingResults,IEnumerable<Customer> customers)
    {
        var merged = new List<DuplicateResult>(existingResults);
        var list = customers.ToList();
        var groups = list
            .Where(c => !string.IsNullOrWhiteSpace(c.Address))
            .GroupBy(c => StringSimilarity.NormalizeAddress(c.Address),
                     StringComparer.Ordinal)
            .Where(g => g.Key.Length > 0 && g.Count() > 1);

        int seq = 0;
        foreach (var group in groups)
        {
            var members = group.ToList();
            for (int i = 0; i < members.Count; i++)
            for (int j = i + 1; j < members.Count; j++)
            {
                var a = members[i];
                var b = members[j];
                var existing = merged.FirstOrDefault(r =>
                    (r.RecordAId == a.Id && r.RecordBId == b.Id) ||
                    (r.RecordAId == b.Id && r.RecordBId == a.Id));

                if (existing is not null && existing.Classification == DuplicateClassification.LikelyDuplicate)
                {
                    continue;
                }

                var local = new DuplicateResult
                {
                    PairId    = $"ADDR{++seq:D4}",
                    RecordAId = a.Id,
                    RecordBId = b.Id,
                    Confidence = 75,
                    Classification = DuplicateClassification.PossibleDuplicate,
                    RowHighlight  = RowHighlight.PossibleDuplicate,
                    GroupId       = $"ADDR-GROUP-{group.Key}",
                    Summary       = "Same normalized address as another record.",
                    SupportingSignals = new()
                    {
                        new SupportingSignal
                        {
                            Field = "address",
                            Strength = "moderate",
                            Reason  = $"Both records share the address “{a.Address}”."
                        }
                    }
                };

                if (existing is null)
                {
                    merged.Add(local);
                }
                else
                {
                    var idx = merged.IndexOf(existing);
                    merged[idx] = local;
                }
            }
        }

        return merged;
    }

    /// <summary>
    /// Build (but do not apply) all identical-address pair results across
    /// <paramref name="universe"/>. Returns PossibleDuplicate at 75%.
    /// Used by the live-typing and live-edit paths, which don't want to
    /// mutate the AI's result list — they just want to paint the
    /// matching rows directly.
    /// </summary>
    private static List<DuplicateResult> BuildIdenticalAddressResults(IEnumerable<Customer> universe,IEnumerable<Customer> _)
    {
        var list = universe.ToList();
        var byAddress = list
            .Where(c => !string.IsNullOrWhiteSpace(c.Address))
            .GroupBy(c => StringSimilarity.NormalizeAddress(c.Address),
                     StringComparer.Ordinal)
            .Where(g => g.Key.Length > 0 && g.Count() > 1);

        var results = new List<DuplicateResult>();
        int seq = 0;
        foreach (var group in byAddress)
        {
            var members = group.ToList();
            for (int i = 0; i < members.Count; i++)
            for (int j = i + 1; j < members.Count; j++)
            {
                var a = members[i];
                var b = members[j];
                results.Add(new DuplicateResult
                {
                    PairId    = $"ADDR{++seq:D4}",
                    RecordAId = a.Id,
                    RecordBId = b.Id,
                    Confidence = 75,
                    Classification = DuplicateClassification.PossibleDuplicate,
                    RowHighlight  = RowHighlight.PossibleDuplicate,
                    GroupId       = $"ADDR-GROUP-{group.Key}",
                    Summary       = "Same normalized address as another record.",
                    SupportingSignals = new()
                    {
                        new SupportingSignal
                        {
                            Field = "address",
                            Strength = "moderate",
                            Reason  = $"Both records share the address “{a.Address}”."
                        }
                    }
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Captures the "duplicate signals" of a customer at the moment it's
    /// being deleted, so that after removal we can identify which other
    /// (still highlighted) customers were only matched against this row and
    /// now need their highlight cleared. A signal is a normalized key that,
    /// if shared with another customer, makes them a candidate duplicate.
    /// We use the same primitives the AI path uses: identical normalized
    /// address, identical normalized email, and exact name/phone pairs.
    /// </summary>
    private static HashSet<string> BuildDuplicateSignals(Customer c)
    {
        var signals = new HashSet<string>(StringComparer.Ordinal);

        var addr = StringSimilarity.NormalizeAddress(c.Address);
        if (addr.Length > 0) signals.Add("addr:" + addr);

        var email = (c.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (email.Length > 0) signals.Add("email:" + email);

        var phoneDigits = new string((c.Phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (phoneDigits.Length >= 7) signals.Add("phone:" + phoneDigits);

        return signals;
    }

    /// <summary>
    /// Called after a row has been removed from <see cref="Customers"/>.
    /// For every still-highlighted row, recompute the same "duplicate
    /// signals" and check whether the deleted row's signals intersect that
    /// row's signals, AND whether any other remaining row in the grid still
    /// shares at least one signal with it. If not, the highlight was only
    /// there because of the deleted row and must be cleared.
    /// </summary>
    private void ClearOrphanedHighlightsAfterDelete(HashSet<string> deletedSignals)
    {
        if (deletedSignals.Count == 0) 
            return;

        foreach (var survivor in Customers)
        {
            if (survivor.RowHighlight == RowHighlight.None) 
                continue;

            var survivorSignals = BuildDuplicateSignals(survivor);
            if (survivorSignals.Count == 0) 
                continue;

            if (!deletedSignals.Overlaps(survivorSignals)) 
                continue;

            var stillHasPartner = Customers.Any(other =>
                other.Id != survivor.Id &&
                BuildDuplicateSignals(other).Overlaps(survivorSignals));

            if (!stillHasPartner)
            {
                survivor.RowHighlight = RowHighlight.None;
                survivor.Match = "—";
                survivor.Status = "Unique";
                survivor.IsEditing = false;
            }
        }
    }
}
