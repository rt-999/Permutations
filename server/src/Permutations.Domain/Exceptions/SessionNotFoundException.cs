namespace Permutations.Domain.Exceptions;

public sealed class SessionNotFoundException : Exception
{
    public SessionNotFoundException() : base("Session not found") { }
    public SessionNotFoundException(string message) : base(message) { }
}
