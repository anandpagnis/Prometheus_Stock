using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PrometheusStock.Tests.Integration;

/// <summary>
/// Boots the API in-memory with <see cref="WebApplicationFactory{TEntryPoint}"/> and
/// exercises the real HTTP pipeline. Template for future endpoint integration tests.
/// </summary>
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_health_returns_200_ok()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
