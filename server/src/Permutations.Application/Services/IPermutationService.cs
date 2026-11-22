using Permutations.Application.Models;

namespace Permutations.Application.Services;

public interface IPermutationService
{
    Task<StartResult> StartAsync(int n, CancellationToken ct);
    Task<NextResult> NextAsync(string sessionId, CancellationToken ct);
    Task<PageResult> GetPageAsync(string sessionId, long page, int pageSize, CancellationToken ct);
    Task ExitBrowseAsync(string sessionId, CancellationToken ct);
}
