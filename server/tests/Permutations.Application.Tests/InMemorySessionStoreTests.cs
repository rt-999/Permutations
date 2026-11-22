using Permutations.Domain.Exceptions;
using Permutations.Domain.Sessions;
using Permutations.Infrastructure.Sessions;

namespace Permutations.Application.Tests;

public class InMemorySessionStoreTests
{
    private sealed class FakeTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    // Fully qualified because 'Options' would otherwise resolve to the namespace Permutations.Application.Options.
    private static InMemorySessionStore CreateStore(FakeTime? time = null, bool sliding = true) =>
        new(Microsoft.Extensions.Options.Options.Create(new SessionStoreOptions
        {
            Ttl = TimeSpan.FromMinutes(30),
            Sliding = sliding
        }), time);

    private static SessionState NewState() => new() { N = 3, Total = 6 };

    [Fact]
    public async Task Use_PersistsChangesBetweenCalls()
    {
        var store = CreateStore();
        var id = await store.CreateAsync(NewState(), default);

        await store.UseAsync(id, s => { s.LastIndex = 5; return 0; }, default);
        var read = await store.UseAsync(id, s => s.LastIndex, default);

        Assert.Equal(5, read);
    }

    [Fact]
    public async Task Use_UnknownSession_Throws()
    {
        var store = CreateStore();
        await Assert.ThrowsAsync<SessionNotFoundException>(() => store.UseAsync("nope", s => 0, default));
    }

    [Fact]
    public async Task Sessions_AreIndependent()
    {
        var store = CreateStore();
        var a = await store.CreateAsync(NewState(), default);
        var b = await store.CreateAsync(NewState(), default);

        await store.UseAsync(a, s => { s.LastIndex = 3; return 0; }, default);

        Assert.Equal(0, await store.UseAsync(b, s => s.LastIndex, default));
    }

    [Fact]
    public async Task ParallelUse_OnSameSession_DoesNotLoseUpdates()
    {
        var store = CreateStore();
        var id = await store.CreateAsync(NewState(), default);

        var tasks = Enumerable.Range(0, 1000)
            .Select(_ => Task.Run(() => store.UseAsync(id, s => { s.LastIndex++; return 0; }, default)));
        await Task.WhenAll(tasks);

        Assert.Equal(1000, await store.UseAsync(id, s => s.LastIndex, default));
    }

    [Fact]
    public async Task Remove_MakesSessionNotFound()
    {
        var store = CreateStore();
        var id = await store.CreateAsync(NewState(), default);

        await store.RemoveAsync(id, default);

        await Assert.ThrowsAsync<SessionNotFoundException>(() => store.UseAsync(id, s => 0, default));
    }

    [Fact]
    public async Task Expired_Session_IsNotFound()
    {
        var time = new FakeTime();
        var store = CreateStore(time);
        var id = await store.CreateAsync(NewState(), default);

        time.Now = time.Now.AddMinutes(31);

        await Assert.ThrowsAsync<SessionNotFoundException>(() => store.UseAsync(id, s => 0, default));
    }

    [Fact]
    public async Task Sliding_ExtendsLifetimeOnUse()
    {
        var time = new FakeTime();
        var store = CreateStore(time, sliding: true);
        var id = await store.CreateAsync(NewState(), default);

        time.Now = time.Now.AddMinutes(20);
        await store.UseAsync(id, s => 0, default);   // refreshes the timer
        time.Now = time.Now.AddMinutes(20);          // 40 minutes total, but only 20 since last use

        await store.UseAsync(id, s => 0, default);   // still alive
    }
}
