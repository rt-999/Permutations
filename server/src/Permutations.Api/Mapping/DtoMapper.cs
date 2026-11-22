using System.Globalization;
using Permutations.Api.Dtos;
using Permutations.Application.Models;

namespace Permutations.Api.Mapping;

/// <summary>The only place where long becomes string (the wire format for big numbers).</summary>
public static class DtoMapper
{
    public static StartResponse ToDto(this StartResult r) =>
        new(r.SessionId, r.N, Str(r.Total));

    public static NextResponse ToDto(this NextResult r) =>
        new(Str(r.Index), r.Permutation);

    public static PageResponse ToDto(this PageResult r) =>
        new(
            Str(r.Page),
            Str(r.TotalPages),
            Str(r.FirstIndex),
            r.Items.Select(i => new PageItemDto(Str(i.Index), i.Permutation)).ToList());

    private static string Str(long value) => value.ToString(CultureInfo.InvariantCulture);
}
