namespace Permutations.Domain.Sessions;

public sealed class SessionState
{
    public int N { get; init; }
    public long Total { get; init; }          // n!
    public long LastIndex { get; set; }       // 1-based. 0 = nothing shown yet
    public int[]? Current { get; set; }       // used by Next
    public BrowseMode Mode { get; set; }
    public long BrowseAnchor { get; set; }    // reference point for page numbering
}
