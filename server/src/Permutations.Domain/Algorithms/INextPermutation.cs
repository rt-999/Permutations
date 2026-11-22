namespace Permutations.Domain.Algorithms;

public interface INextPermutation
{
    /// <summary>Advances the array in place. Returns false if it was the last permutation.</summary>
    bool TryAdvance(int[] permutation);
}
