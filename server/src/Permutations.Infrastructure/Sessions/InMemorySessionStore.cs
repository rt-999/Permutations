using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Permutations.Application.Abstractions;
using Permutations.Domain.Exceptions;
using Permutations.Domain.Sessions;

namespace Permutations.Infrastructure.Sessions;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, Entry> _sessions = new();
    private readonly SessionStoreOptions _options;
    private readonly TimeProvider _time;

    // TimeProvider is built into .NET 8. If nothing is registered, the real clock is used.
    public InMemorySessionStore(IOptions<SessionStoreOptions> options, TimeProvider? timeProvider = null)
    {
        _options = options.Value;
        _time = timeProvider ?? TimeProvider.System;
    }

    public Task<string> CreateAsync(SessionState state, CancellationToken ct)
    {
        var entry = new Entry(state, _time.GetUtcNow());

        while (true)
        {
            // 128 random bits from a cryptographic generator: impossible to guess.
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            if (_sessions.TryAdd(id, entry))
                return Task.FromResult(id);
        }
    }

    public async Task<T> UseAsync<T>(string sessionId, Func<SessionState, T> work, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(sessionId) || !_sessions.TryGetValue(sessionId, out var entry))
            throw new SessionNotFoundException();

        await entry.Lock.WaitAsync(ct);   // take the key; wait in line if someone else has it
        try
        {
            // The session may have been removed while we were waiting in line, or may have expired.
            if (entry.Removed || IsExpired(entry, _time.GetUtcNow()))
            {
                _sessions.TryRemove(new KeyValuePair<string, Entry>(sessionId, entry));
                entry.Removed = true;
                throw new SessionNotFoundException();
            }

            var result = work(entry.State);   // the atomic read-modify-write

            if (_options.Sliding)
                entry.LastAccess = _time.GetUtcNow();   // every use extends the session's life
            return result;
        }
        finally
        {
            entry.Lock.Release();   // always, even on exception
        }
    }

    public Task RemoveAsync(string sessionId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(sessionId) && _sessions.TryRemove(sessionId, out var entry))
            entry.Removed = true;
        return Task.CompletedTask;
    }

    /// <summary>Called periodically by SessionCleanupService.</summary>
    internal void RemoveExpired()
    {
        var now = _time.GetUtcNow();
        foreach (var pair in _sessions)
        {
            if (pair.Value.Lock.CurrentCount == 0) continue;   // someone is using it right now, skip
            if (!IsExpired(pair.Value, now)) continue;

            // Removes only if the dictionary still holds this exact entry.
            if (_sessions.TryRemove(pair))
                pair.Value.Removed = true;
        }
    }

    private bool IsExpired(Entry entry, DateTimeOffset now) => now - entry.LastAccess > _options.Ttl;

    private sealed class Entry(SessionState state, DateTimeOffset now)
    {
        public SessionState State { get; } = state;
        public SemaphoreSlim Lock { get; } = new(1, 1);   // one key
        public DateTimeOffset LastAccess { get; set; } = now;
        public volatile bool Removed;                      // volatile: visible to all threads immediately
    }
}
