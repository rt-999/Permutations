namespace Permutations.Domain.Exceptions;

public sealed class PermutationsExhaustedException : Exception
{
    public PermutationsExhaustedException() : base("No more permutations") { }
    public PermutationsExhaustedException(string message) : base(message) { }
}
