namespace Permutations.Application.Options;

public sealed class PermutationLimits
{
    public const string Section = "PermutationLimits";
    public int MinN { get; set; } = 1;
    public int MaxN { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
}
