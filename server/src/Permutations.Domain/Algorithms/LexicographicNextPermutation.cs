namespace Permutations.Domain.Algorithms;

/// <summary>Classic "next lexicographic permutation". O(n) per step, in place, no allocations.</summary>
public sealed class LexicographicNextPermutation : INextPermutation
{
    public bool TryAdvance(int[] permutation)
    {
        ArgumentNullException.ThrowIfNull(permutation);

        // 1. Find the pivot: the last position whose value is smaller than its right neighbor.
        var pivot = permutation.Length - 2;
        while (pivot >= 0 && permutation[pivot] >= permutation[pivot + 1])
            pivot--;

        if (pivot < 0)
            return false;   // fully descending = last permutation; the array is left untouched

        // 2. Find the smallest value to the right that is still greater than the pivot (the last such one).
        var successor = permutation.Length - 1;
        while (permutation[successor] <= permutation[pivot])
            successor--;

        // 3. Swap them.
        (permutation[pivot], permutation[successor]) = (permutation[successor], permutation[pivot]);

        // 4. The suffix is descending; reverse it to make it the smallest possible (ascending).
        Array.Reverse(permutation, pivot + 1, permutation.Length - pivot - 1);
        return true;
    }
}
