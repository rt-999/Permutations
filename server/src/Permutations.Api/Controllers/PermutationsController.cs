using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Permutations.Api.Dtos;
using Permutations.Api.Mapping;
using Permutations.Application.Services;
using Permutations.Domain.Exceptions;

namespace Permutations.Api.Controllers;

[ApiController]
[Route("api/permutations")]
public sealed class PermutationsController(IPermutationService service) : ControllerBase
{
    private const int DefaultPageSize = 20;

    [HttpPost("start")]
    public async Task<ActionResult<StartResponse>> Start([FromBody] StartRequest request, CancellationToken ct)
    {
        var result = await service.StartAsync(request.N, ct);
        return Ok(result.ToDto());
    }

    [HttpPost("next")]
    public async Task<ActionResult<NextResponse>> Next(
        [FromHeader(Name = SessionIdHeader.Name)] string sessionId, CancellationToken ct)
    {
        var result = await service.NextAsync(sessionId, ct);
        return Ok(result.ToDto());
    }

    [HttpGet("all")]
    public async Task<ActionResult<PageResponse>> All(
        [FromHeader(Name = SessionIdHeader.Name)] string sessionId,
        [FromQuery] string page = "1",
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken ct = default)
    {
        // page arrives as a string because it can exceed what a JavaScript number holds.
        // NumberStyles.None: digits only (no sign, spaces or decimal point).
        if (!long.TryParse(page, NumberStyles.None, CultureInfo.InvariantCulture, out var pageNumber))
            throw new InvalidPageException("page must be a positive whole number.");

        var result = await service.GetPageAsync(sessionId, pageNumber, pageSize, ct);
        return Ok(result.ToDto());
    }

    [HttpPost("all/exit")]
    public async Task<IActionResult> Exit(
        [FromHeader(Name = SessionIdHeader.Name)] string sessionId, CancellationToken ct)
    {
        await service.ExitBrowseAsync(sessionId, ct);
        return NoContent();
    }
}
