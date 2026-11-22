using Permutations.Domain.Exceptions;

namespace Permutations.Domain.Algorithms;

/// <summary>Computes n! for 0 &lt;= n &lt;= 20. 20! = 2,432,902,008,176,640,000 still fits in a long.</summary>
public static class FactorialCalculator
{
    public const int MaxSupported = 20;

    // Computed once, so each call is O(1).
    private static readonly long[] Table = BuildTable();

    public static long Of(int n)
    {
        if (n < 0 || n > MaxSupported)
            throw new InvalidNException($"n must be between 0 and {MaxSupported} (was {n}).");
        return Table[n];
    }

    private static long[] BuildTable()
    {
        var table = new long[MaxSupported + 1];
        table[0] = 1;
        for (var i = 1; i <= MaxSupported; i++)
            table[i] = checked(table[i - 1] * i);   // 'checked' fails loudly on overflow instead of wrapping silently
        return table;
    }
}
