namespace Permutations.Api.Dtos;

/// <summary>Large numbers are strings on the wire.</summary>
public sealed record StartResponse(string SessionId, int N, string Total);
