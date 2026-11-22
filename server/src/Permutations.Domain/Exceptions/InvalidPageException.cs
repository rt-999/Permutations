namespace Permutations.Domain.Exceptions;

public sealed class InvalidPageException : Exception
{
    public InvalidPageException() : base("Invalid page") { }
    public InvalidPageException(string message) : base(message) { }
}
