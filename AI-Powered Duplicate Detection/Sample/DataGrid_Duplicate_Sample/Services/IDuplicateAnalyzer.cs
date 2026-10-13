using DataGrid_Duplicate_Sample.Analysis;
using DataGrid_Duplicate_Sample.Models;

namespace DataGrid_Duplicate_Sample.Services;

public interface IDuplicateAnalyzer
{
    Task<List<DuplicateResult>> AnalyzeAsync(IReadOnlyList<CustomerPair> candidatePairs, CancellationToken cancellationToken = default);
}
