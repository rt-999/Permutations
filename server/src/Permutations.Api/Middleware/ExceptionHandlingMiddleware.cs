using Microsoft.AspNetCore.Mvc;
using Permutations.Domain.Exceptions;

namespace Permutations.Api.Middleware;

/// <summary>Maps exceptions to RFC 7807 ProblemDetails responses.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private sealed record Mapped(int Status, string Code, string Title);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away. Nobody is listening for a response; not an error.
            logger.LogDebug("Request was cancelled by the client.");
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;   // too late to change the response; let the server deal with it

            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var mapped = Map(ex);

        if (mapped is null)
        {
            logger.LogError(ex, "Unhandled exception");
            mapped = new Mapped(StatusCodes.Status500InternalServerError, "unexpected_error", "Unexpected error");
        }
        else
        {
            logger.LogInformation("Handled {ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
        }

        var problem = new ProblemDetails
        {
            Status = mapped.Status,
            Title = mapped.Title,
            // Domain messages are written for users. For unexpected errors we never leak internals.
            Detail = mapped.Status == StatusCodes.Status500InternalServerError ? null : ex.Message,
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = mapped.Code;

        context.Response.Clear();
        context.Response.StatusCode = mapped.Status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }

    private static Mapped? Map(Exception ex) => ex switch
    {
        InvalidNException => new(StatusCodes.Status400BadRequest, "invalid_n", "Invalid n"),
        InvalidPageException => new(StatusCodes.Status400BadRequest, "invalid_page", "Invalid page"),
        SessionNotFoundException => new(StatusCodes.Status404NotFound, "session_not_found", "Session not found"),
        PermutationsExhaustedException => new(StatusCodes.Status409Conflict, "permutations_exhausted", "No more permutations"),
        _ => null
    };
}
