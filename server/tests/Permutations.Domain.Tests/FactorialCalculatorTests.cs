using Permutations.Domain.Algorithms;
using Permutations.Domain.Exceptions;

namespace Permutations.Domain.Tests;

public class FactorialCalculatorTests
{
    [Theory]
    [InlineData(0, 1L)]
    [InlineData(1, 1L)]
    [InlineData(5, 120L)]
    [InlineData(10, 3_628_800L)]
    [InlineData(20, 2_432_902_008_176_640_000L)]
    public void Of_ReturnsKnownValues(int n, long expected) => Assert.Equal(expected, FactorialCalculator.Of(n));

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Of_OutOfRange_Throws(int n) => Assert.Throws<InvalidNException>(() => FactorialCalculator.Of(n));
}
