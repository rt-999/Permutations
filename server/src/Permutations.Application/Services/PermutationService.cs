using Microsoft.Extensions.Options;
using Permutations.Application.Abstractions;
using Permutations.Application.Models;
using Permutations.Application.Options;
using Permutations.Application.Paging;
using Permutations.Domain.Algorithms;
using Permutations.Domain.Exceptions;
using Permutations.Application.Sessions;

namespace Permutations.Application.Services;

public sealed class PermutationService(
    ISessionStore store,
    INextPermutation nextPermutation,
    IPermutationUnranker unranker,
    IOptions<PermutationLimits> limitsOptions) : IPermutationService
{
    private PermutationLimits Limits => limitsOptions.Value;

    public async Task<StartResult> StartAsync(int n, CancellationToken ct)
    {
        if (n < Limits.MinN || n > Limits.MaxN)
            throw new InvalidNException($"n must be between {Limits.MinN} and {Limits.MaxN}.");

        var state = new SessionState
        {
            N = n,
            Total = FactorialCalculator.Of(n),
            LastIndex = 0,
            Current = null,
            Mode = BrowseMode.Single,
            BrowseAnchor = 0
        };

        var sessionId = await store.CreateAsync(state, ct);
        return new StartResult(sessionId, n, state.Total);
    }

    public Task<NextResult> NextAsync(string sessionId, CancellationToken ct) =>
        store.UseAsync(sessionId, state =>
        {
            // 1. Validate and compute everything first, without touching the state.
            if (state.LastIndex >= state.Total)
                throw new PermutationsExhaustedException();

            int[] following;
            if (state.Current is null)
            {
                following = unranker.Unrank(state.N, 0);              // first call: 1,2,...,n
            }
            else
            {
                following = (int[])state.Current.Clone();             // advance a copy, never the stored array
                if (!nextPermutation.TryAdvance(following))
                    throw new PermutationsExhaustedException();
            }

            // 2. Only now commit.
            state.Current = following;
            state.LastIndex++;
            return new NextResult(state.LastIndex, following);
        }, ct);

    public Task<PageResult> GetPageAsync(string sessionId, long page, int pageSize, CancellationToken ct)
    {
        // Input checks that don't need the session: fail fast, before taking any lock.
        if (pageSize < 1 || pageSize > Limits.MaxPageSize)
            throw new InvalidPageException($"pageSize must be between 1 and {Limits.MaxPageSize}.");
        if (page < 1)
            throw new InvalidPageException("page must be 1 or greater.");

        return store.UseAsync(sessionId, state =>
        {
            // 1. Decide the anchor (a local variable; the state is not changed yet).
            var anchor = state.Mode == BrowseMode.Single ? state.LastIndex : state.BrowseAnchor;

            var totalPages = PageCalculator.TotalPages(state.Total, anchor, pageSize);
            if (totalPages == 0)
                throw new PermutationsExhaustedException();
            if (page > totalPages)
                throw new InvalidPageException($"page must be between 1 and {totalPages}.");

            var firstIndex = PageCalculator.FirstIndex(anchor, page, pageSize);
            var count = PageCalculator.Count(state.Total, firstIndex, pageSize);

            // 2. Hybrid: Unrank for the first item, then NextPermutation for the rest.
            var working = unranker.Unrank(state.N, firstIndex - 1);
            var items = new List<PageItem>(count);
            items.Add(new PageItem(firstIndex, (int[])working.Clone()));

            for (var i = 1; i < count; i++)
            {
                if (!nextPermutation.TryAdvance(working))
                    throw new InvalidOperationException("Ran out of permutations inside a page; paging math is wrong.");
                items.Add(new PageItem(firstIndex + i, (int[])working.Clone()));
            }

            // 3. Everything succeeded: commit.
            var last = items[^1];
            state.BrowseAnchor = anchor;
            state.Mode = BrowseMode.Browsing;
            state.LastIndex = last.Index;
            state.Current = last.Permutation;

            return new PageResult(page, totalPages, firstIndex, items);
        }, ct);
    }

    public Task ExitBrowseAsync(string sessionId, CancellationToken ct) =>
        store.UseAsync(sessionId, state =>
        {
            state.Mode = BrowseMode.Single;   // Current and LastIndex are already correct
            return 0;
        }, ct);
}
