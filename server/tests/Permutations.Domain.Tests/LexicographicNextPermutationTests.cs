using Permutations.Domain.Algorithms;

namespace Permutations.Domain.Tests;

public class LexicographicNextPermutationTests
{
    private readonly LexicographicNextPermutation _sut = new();

    [Fact]
    public void TryAdvance_ProducesAllPermutationsOfThreeInOrder()
    {
        var p = new[] { 1, 2, 3 };
        var seen = new List<string> { string.Join("", p) };
        while (_sut.TryAdvance(p)) seen.Add(string.Join("", p));

        Assert.Equal(new[] { "123", "132", "213", "231", "312", "321" }, seen);
    }

    [Fact]
    public void TryAdvance_OnLast_ReturnsFalseAndLeavesArrayUnchanged()
    {
        var p = new[] { 3, 2, 1 };
        Assert.False(_sut.TryAdvance(p));
        Assert.Equal(new[] { 3, 2, 1 }, p);
    }

    [Fact]
    public void TryAdvance_SingleElement_ReturnsFalse() => Assert.False(_sut.TryAdvance(new[] { 1 }));

    [Fact]
    public void TryAdvance_CountMatchesFactorial()
    {
        var p = new[] { 1, 2, 3, 4, 5, 6 };
        long count = 1;
        while (_sut.TryAdvance(p)) count++;
        Assert.Equal(720, count);
    }
}
