namespace Permutations.Infrastructure.Sessions;

public sealed class SessionStoreOptions
{
    public const string Section = "SessionStore";
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(30);
    public bool Sliding { get; set; } = true;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);
}
