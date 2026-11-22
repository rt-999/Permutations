namespace Permutations.Domain.Algorithms;

public interface IPermutationUnranker
{
    /// <summary>index is 0-based, lexicographic order. Returns values 1..n.</summary>
    int[] Unrank(int n, long index);
}
