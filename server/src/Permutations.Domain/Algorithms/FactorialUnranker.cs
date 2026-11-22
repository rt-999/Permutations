using Permutations.Domain.Exceptions;

namespace Permutations.Domain.Algorithms;

/// <summary>
/// Computes the permutation at a given 0-based index directly (lexicographic order),
/// using the Factorial Number System. Complexity: O(n^2).
/// </summary>
public sealed class FactorialUnranker : IPermutationUnranker
{
    public int[] Unrank(int n, long index)
    {
        if (n < 1 || n > FactorialCalculator.MaxSupported)
            throw new InvalidNException($"n must be between 1 and {FactorialCalculator.MaxSupported} (was {n}).");

        var total = FactorialCalculator.Of(n);
        if (index < 0 || index >= total)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"index must be in [0, {total - 1}].");

        // The numbers still available, always kept in ascending order.
        var available = new List<int>(n);
        for (var v = 1; v <= n; v++) available.Add(v);

        var result = new int[n];
        for (var position = 0; position < n; position++)
        {
            var blockSize = FactorialCalculator.Of(n - 1 - position); // permutations per choice at this position
            var choice = (int)(index / blockSize);                    // which of the available numbers
            index %= blockSize;                                       // remainder goes to the next positions

            result[position] = available[choice];
            available.RemoveAt(choice);
        }
        return result;
    }
}
