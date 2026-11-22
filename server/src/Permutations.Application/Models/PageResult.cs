namespace Permutations.Application.Models;

public sealed record PageItem(long Index, int[] Permutation);

public sealed record PageResult(long Page, long TotalPages, long FirstIndex, IReadOnlyList<PageItem> Items);
