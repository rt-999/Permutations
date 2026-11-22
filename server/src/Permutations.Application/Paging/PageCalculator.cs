namespace Permutations.Application.Paging;

/// <summary>
/// Pure paging math. All indexes are 1-based.
/// anchor = last index shown before entering browse mode (0 = nothing shown yet).
/// </summary>
public static class PageCalculator
{
    /// <summary>How many pages exist after the anchor. 0 when nothing remains.</summary>
    public static long TotalPages(long total, long anchor, int pageSize)
    {
        EnsurePageSize(pageSize);

        var remaining = total - anchor;
        if (remaining <= 0) return 0;

        // Integer ceiling division: (a + b - 1) / b
        return (remaining + pageSize - 1) / pageSize;
    }

    /// <summary>Index of the first permutation on the given page (page is 1-based).</summary>
    public static long FirstIndex(long anchor, long page, int pageSize)
    {
        EnsurePageSize(pageSize);
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), page, "page must be >= 1.");

        return checked(anchor + (page - 1) * pageSize + 1);
    }

    /// <summary>How many permutations are on the page. Smaller than pageSize on the last page.</summary>
    public static int Count(long total, long firstIndex, int pageSize)
    {
        EnsurePageSize(pageSize);

        var left = total - firstIndex + 1;
        if (left <= 0) return 0;

        return (int)Math.Min(pageSize, left);
    }

    private static void EnsurePageSize(int pageSize)
    {
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "pageSize must be >= 1.");
    }
}
