using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using PrometheusStock.Api.MarketData;

namespace PrometheusStock.Api.Common;

/// <summary>
/// Translates the market-data domain exceptions into RFC 7807 responses:
/// <see cref="SymbolNotFoundException" /> → 404, <see cref="UpstreamException" /> → 502.
/// The 502 is deliberately opaque — a 5xx must not echo upstream URLs, status codes or
/// messages back to the caller; the real cause is logged server-side instead. Anything
/// else is left for the framework's default 500 handling.
/// </summary>
internal sealed class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int status;
        string title;
        string? detail = null;

        switch (exception)
        {
            case SymbolNotFoundException:
                status = StatusCodes.Status404NotFound;
                title = "Symbol not found";
                detail = exception.Message;
                break;

            case UpstreamException:
                status = StatusCodes.Status502BadGateway;
                title = "Upstream data provider error";
                break;

            default:
                return false;
        }

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Request to {Path} failed upstream; returning {Status}",
                httpContext.Request.Path, status);
        }
        else
        {
            logger.LogInformation("Request to {Path} returned {Status}: {Message}",
                httpContext.Request.Path, status, exception.Message);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
            },
        });
    }
}
