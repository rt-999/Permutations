using Permutations.Application.Options;
using Permutations.Application.Services;
using Permutations.Domain.Algorithms;
using Permutations.Domain.Exceptions;
using Permutations.Infrastructure.Sessions;

namespace Permutations.Application.Tests;

public class PermutationServiceTests
{
    private readonly FactorialUnranker _unranker = new();

    // Real store and real algorithms: these are integration-style tests of the whole Application layer.
    private static PermutationService CreateService(int maxPageSize = 100) =>
        new(
            new InMemorySessionStore(Microsoft.Extensions.Options.Options.Create(new SessionStoreOptions())),
            new LexicographicNextPermutation(),
            new FactorialUnranker(),
            Microsoft.Extensions.Options.Options.Create(new PermutationLimits { MaxPageSize = maxPageSize }));

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task Start_InvalidN_Throws(int n)
        => await Assert.ThrowsAsync<InvalidNException>(() => CreateService().StartAsync(n, default));

    [Fact]
    public async Task Start_ReturnsFactorialAsTotal()
    {
        var result = await CreateService().StartAsync(5, default);
        Assert.Equal(120, result.Total);
        Assert.False(string.IsNullOrEmpty(result.SessionId));
    }

    [Fact]
    public async Task Next_ReturnsAllPermutationsInOrder_ThenExhausted()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(3, default)).SessionId;

        var seen = new List<string>();
        for (var i = 1; i <= 6; i++)
        {
            var r = await sut.NextAsync(id, default);
            Assert.Equal(i, r.Index);
            seen.Add(string.Join("", r.Permutation));
        }

        Assert.Equal(new[] { "123", "132", "213", "231", "312", "321" }, seen);
        await Assert.ThrowsAsync<PermutationsExhaustedException>(() => sut.NextAsync(id, default));
    }

    [Fact]
    public async Task GetPage_FirstPage_ReturnsFirstItems_AndCorrectTotalPages()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;   // 24 permutations

        var page = await sut.GetPageAsync(id, 1, 10, default);

        Assert.Equal(3, page.TotalPages);
        Assert.Equal(1, page.FirstIndex);
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, page.Items[0].Permutation);
    }

    [Fact]
    public async Task GetPage_LastPage_IsPartial()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;

        var page = await sut.GetPageAsync(id, 3, 10, default);

        Assert.Equal(4, page.Items.Count);
        Assert.Equal(24, page.Items[^1].Index);
        Assert.Equal(new[] { 4, 3, 2, 1 }, page.Items[^1].Permutation);
    }

    [Fact]
    public async Task GetPage_EveryItemMatchesUnrank()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;

        for (long p = 1; p <= 5; p++)   // 24 items, pageSize 5 -> 5 pages
        {
            var page = await sut.GetPageAsync(id, p, 5, default);
            foreach (var item in page.Items)
                Assert.Equal(_unranker.Unrank(4, item.Index - 1), item.Permutation);
        }
    }

    [Fact]
    public async Task GetPage_AfterNext_NumbersPagesFromTheAnchor()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;
        for (var i = 0; i < 5; i++) await sut.NextAsync(id, default);   // LastIndex = 5

        var page = await sut.GetPageAsync(id, 1, 10, default);

        Assert.Equal(6, page.FirstIndex);          // first page starts right after the anchor
        Assert.Equal(2, page.TotalPages);          // ceil((24 - 5) / 10)
        Assert.Equal(_unranker.Unrank(4, 5), page.Items[0].Permutation);
    }

    [Fact]
    public async Task ExitBrowse_ThenNext_ContinuesAfterLastShown()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;

        await sut.GetPageAsync(id, 2, 10, default);   // items 11..20
        await sut.ExitBrowseAsync(id, default);
        var next = await sut.NextAsync(id, default);

        Assert.Equal(21, next.Index);
        Assert.Equal(_unranker.Unrank(4, 20), next.Permutation);
    }

    [Fact]
    public async Task GetPage_PageOutOfRange_Throws_AndDoesNotCorruptState()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;

        await Assert.ThrowsAsync<InvalidPageException>(() => sut.GetPageAsync(id, 99, 10, default));
        await Assert.ThrowsAsync<InvalidPageException>(() => sut.GetPageAsync(id, 0, 10, default));

        var next = await sut.NextAsync(id, default);   // state untouched: still the very first permutation
        Assert.Equal(1, next.Index);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetPage_InvalidPageSize_Throws(int pageSize)
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(4, default)).SessionId;
        await Assert.ThrowsAsync<InvalidPageException>(() => sut.GetPageAsync(id, 1, pageSize, default));
    }

    [Fact]
    public async Task AfterBrowsingEverything_Next_ReportsExhausted_ButPagesRemainViewable()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(3, default)).SessionId;

        await sut.GetPageAsync(id, 1, 10, default);   // shows all 6, now in Browsing mode

        await Assert.ThrowsAsync<PermutationsExhaustedException>(() => sut.NextAsync(id, default));

        // The browse view is still valid: the user can go back to page 1.
        var again = await sut.GetPageAsync(id, 1, 10, default);
        Assert.Equal(6, again.Items.Count);
    }

    [Fact]
    public async Task AfterNextShownEverything_EnteringBrowse_ReportsExhausted()
    {
        var sut = CreateService();
        var id = (await sut.StartAsync(3, default)).SessionId;
        for (var i = 0; i < 6; i++) await sut.NextAsync(id, default);

        await Assert.ThrowsAsync<PermutationsExhaustedException>(() => sut.GetPageAsync(id, 1, 10, default));
    }

    [Fact]
    public async Task UnknownSession_Throws()
        => await Assert.ThrowsAsync<SessionNotFoundException>(() => CreateService().NextAsync("nope", default));
}
