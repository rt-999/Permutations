using Permutations.Domain.Algorithms;
using Permutations.Domain.Exceptions;

namespace Permutations.Domain.Tests;

public class FactorialUnrankerTests
{
    private readonly FactorialUnranker _sut = new();

    [Fact]
    public void Unrank_ThreeElements_MatchesExpectedOrder()
    {
        var expected = new[] { "123", "132", "213", "231", "312", "321" };
        for (var i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], string.Join("", _sut.Unrank(3, i)));
    }

    [Fact]
    public void Unrank_FirstAndLast_ForN20()
    {
        Assert.Equal(Enumerable.Range(1, 20), _sut.Unrank(20, 0));
        Assert.Equal(Enumerable.Range(1, 20).Reverse(), _sut.Unrank(20, FactorialCalculator.Of(20) - 1));
    }

    // Cross-check: the two algorithms must agree on every index for small n.
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    public void Unrank_AgreesWithNextPermutation(int n)
    {
        var next = new LexicographicNextPermutation();
        var current = Enumerable.Range(1, n).ToArray();
        var total = FactorialCalculator.Of(n);

        for (long i = 0; i < total; i++)
        {
            Assert.Equal(current, _sut.Unrank(n, i));
            Assert.Equal(i < total - 1, next.TryAdvance(current));
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(21, 0)]
    public void Unrank_InvalidN_Throws(int n, long index) => Assert.Throws<InvalidNException>(() => _sut.Unrank(n, index));

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void Unrank_IndexOutOfRange_Throws(long index) => Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Unrank(3, index));
}
