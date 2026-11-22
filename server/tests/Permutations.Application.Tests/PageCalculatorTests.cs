using Permutations.Application.Paging;
using Permutations.Domain.Algorithms;

namespace Permutations.Application.Tests;

public class PageCalculatorTests
{
    [Theory]
    [InlineData(120, 0, 20, 6)]       // exact division
    [InlineData(120, 0, 50, 3)]       // last page is partial
    [InlineData(5000, 1000, 100, 40)] // anchor shifts the start
    [InlineData(6, 0, 100, 1)]        // fewer permutations than pageSize
    [InlineData(120, 120, 20, 0)]     // nothing left
    public void TotalPages_ReturnsExpected(long total, long anchor, int pageSize, long expected)
        => Assert.Equal(expected, PageCalculator.TotalPages(total, anchor, pageSize));

    [Theory]
    [InlineData(0, 1, 20, 1)]
    [InlineData(0, 2, 20, 21)]
    [InlineData(1000, 1, 100, 1001)] // the example from the assignment
    [InlineData(1000, 2, 100, 1101)]
    public void FirstIndex_ReturnsExpected(long anchor, long page, int pageSize, long expected)
        => Assert.Equal(expected, PageCalculator.FirstIndex(anchor, page, pageSize));

    [Theory]
    [InlineData(120, 1, 50, 50)]
    [InlineData(120, 101, 50, 20)]    // last, partial page
    [InlineData(6, 1, 100, 6)]
    [InlineData(120, 121, 50, 0)]     // beyond the end
    public void Count_ReturnsExpected(long total, long firstIndex, int pageSize, int expected)
        => Assert.Equal(expected, PageCalculator.Count(total, firstIndex, pageSize));

    [Fact]
    public void LastPage_ForN20_HasFullPageAndNoOverflow()
    {
        var total = FactorialCalculator.Of(20);
        var pages = PageCalculator.TotalPages(total, 0, 100);

        Assert.Equal(24_329_020_081_766_400L, pages);

        var first = PageCalculator.FirstIndex(0, pages, 100);
        Assert.Equal(total - 99, first);
        Assert.Equal(100, PageCalculator.Count(total, first, 100));
    }

    [Fact]
    public void InvalidPageSize_Throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => PageCalculator.TotalPages(10, 0, 0));
}
