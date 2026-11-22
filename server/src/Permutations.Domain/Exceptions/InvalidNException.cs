namespace Permutations.Domain.Exceptions;

public sealed class InvalidNException : Exception
{
    public InvalidNException() : base("n is out of range") { }
    public InvalidNException(string message) : base(message) { }
}
