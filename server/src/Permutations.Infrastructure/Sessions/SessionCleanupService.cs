using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Permutations.Infrastructure.Sessions;

/// <summary>Background service that removes expired sessions so memory does not grow forever.</summary>
public sealed class SessionCleanupService(
    InMemorySessionStore store,
    IOptions<SessionStoreOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.CleanupInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                store.RemoveExpired();
        }
        catch (OperationCanceledException)
        {
            // Normal: the server is shutting down.
        }
    }
}
