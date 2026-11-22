using Permutations.Domain.Sessions;

namespace Permutations.Application.Abstractions;

/// <summary>Port. UseAsync is an atomic read-modify-write per session.</summary>
public interface ISessionStore
{
    Task<string> CreateAsync(SessionState state, CancellationToken ct);
    Task<T> UseAsync<T>(string sessionId, Func<SessionState, T> work, CancellationToken ct);
    Task RemoveAsync(string sessionId, CancellationToken ct);
}
