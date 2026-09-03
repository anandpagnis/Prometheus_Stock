using Microsoft.AspNetCore.Mvc.Testing;

namespace PrometheusStock.Tests.Integration;

/// <summary>
/// The API is browser-facing and the Vite dev server drifts between ports when 5173 is
/// taken, so a CORS preflight from any configured local origin must be answered with
/// the matching <c>Access-Control-Allow-Origin</c> header.
/// </summary>
public sealed class CorsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("http://localhost:5174")]
    public async Task Preflight_from_a_configured_frontend_origin_is_allowed(string origin)
    {
        using HttpRequestMessage preflight = new(HttpMethod.Options, "/api/stocks/TSLA/intraday");
        preflight.Headers.Add("Origin", origin);
        preflight.Headers.Add("Access-Control-Request-Method", "GET");

        HttpResponseMessage response = await factory.CreateClient().SendAsync(preflight);

        response.Headers.GetValues("Access-Control-Allow-Origin").ShouldContain(origin);
    }
}
