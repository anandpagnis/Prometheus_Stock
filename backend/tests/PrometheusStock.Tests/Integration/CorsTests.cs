using Microsoft.AspNetCore.Mvc.Testing;

namespace PrometheusStock.Tests.Integration;

/// <summary>
/// The API is browser-facing, so a CORS preflight from the Vite dev origin must be
/// answered with the matching <c>Access-Control-Allow-Origin</c> header.
/// </summary>
public sealed class CorsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string FrontendOrigin = "http://localhost:5173";

    [Fact]
    public async Task Preflight_from_the_frontend_origin_is_allowed()
    {
        using HttpRequestMessage preflight = new(HttpMethod.Options, "/api/stocks/TSLA/intraday");
        preflight.Headers.Add("Origin", FrontendOrigin);
        preflight.Headers.Add("Access-Control-Request-Method", "GET");

        HttpResponseMessage response = await factory.CreateClient().SendAsync(preflight);

        response.Headers.GetValues("Access-Control-Allow-Origin").ShouldContain(FrontendOrigin);
    }
}
