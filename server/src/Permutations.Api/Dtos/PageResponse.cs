namespace Permutations.Api.Dtos;

public sealed record PageItemDto(string Index, int[] Permutation);

public sealed record PageResponse(string Page, string TotalPages, string FirstIndex, IReadOnlyList<PageItemDto> Items);
