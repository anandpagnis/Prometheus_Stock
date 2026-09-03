using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PrometheusStock.Api.MarketData;

namespace PrometheusStock.Tests.Integration;

/// <summary>
/// <c>GET /api/stocks/{symbol}/intraday</c> booted through the real pipeline with the
/// upstream provider and the aggregator substituted, so these cases pin routing,
/// symbol validation, the provider → aggregator → response mapping, 4 dp banker's
/// rounding and camelCase serialisation. Written test-first.
/// </summary>
public sealed class IntradayEndpointTests : IDisposable
{
    private readonly IStockDataProvider _provider = Substitute.For<IStockDataProvider>();
    private readonly IIntradayAggregator _aggregator = Substitute.For<IIntradayAggregator>();
    private readonly WebApplicationFactory<Program> _factory;

    public IntradayEndpointTests()
    {
        _provider.GetIntradayBarsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<IntradayBar>());
        _aggregator.Aggregate(Arg.Any<IReadOnlyList<IntradayBar>>())
            .Returns([]);

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_provider);
                services.AddSingleton(_aggregator);
            }));
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Returns_rounded_camelCase_aggregates_in_aggregator_order()
    {
        _aggregator.Aggregate(Arg.Any<IReadOnlyList<IntradayBar>>()).Returns(
        [
            new DailyAggregate(new DateOnly(2009, 1, 30), 2.00025m, 2.00035m, 49_073_348L),
            new DailyAggregate(new DateOnly(2009, 2, 2), 10.5m, 20.25m, 100L),
        ]);

        HttpResponseMessage response =
            await _factory.CreateClient().GetAsync("/api/stocks/TSLA/intraday");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using Stream stream = await response.Content.ReadAsStreamAsync();
        using JsonDocument doc = await JsonDocument.ParseAsync(stream);
        JsonElement root = doc.RootElement;

        root.ValueKind.ShouldBe(JsonValueKind.Array);
        root.GetArrayLength().ShouldBe(2);

        JsonElement first = root[0];
        first.GetProperty("day").GetString().ShouldBe("2009-01-30");
        first.GetProperty("lowAverage").GetDecimal().ShouldBe(2.0002m);  // 2.00025 → half to even (2)
        first.GetProperty("highAverage").GetDecimal().ShouldBe(2.0004m); // 2.00035 → half to even (4)
        first.GetProperty("volume").GetInt64().ShouldBe(49_073_348L);

        JsonElement second = root[1];
        second.GetProperty("day").GetString().ShouldBe("2009-02-02");
        second.GetProperty("lowAverage").GetDecimal().ShouldBe(10.5m);
        second.GetProperty("highAverage").GetDecimal().ShouldBe(20.25m);
        second.GetProperty("volume").GetInt64().ShouldBe(100L);
    }

    [Theory]
    [InlineData("%40%40")]           // "@@" — character outside the allowed set
    [InlineData("a%20b")]            // contains a space
    [InlineData("AAAAAAAAAAAAAAAA")] // 16 characters — over the 15 limit
    public async Task Rejects_an_invalid_symbol_with_400_before_any_upstream_call(string symbolSegment)
    {
        HttpResponseMessage response =
            await _factory.CreateClient().GetAsync($"/api/stocks/{symbolSegment}/intraday");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await _provider.DidNotReceiveWithAnyArgs().GetIntradayBarsAsync(default!, default);
    }

    [Theory]
    [InlineData("BRK-B", "BRK-B")]
    [InlineData("%5EGSPC", "^GSPC")]
    public async Task Accepts_punctuated_symbols_and_forwards_them_verbatim(string symbolSegment, string expected)
    {
        HttpResponseMessage response =
            await _factory.CreateClient().GetAsync($"/api/stocks/{symbolSegment}/intraday");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _provider.Received(1).GetIntradayBarsAsync(expected, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Maps_SymbolNotFoundException_to_a_404_problem_document()
    {
        _provider.GetIntradayBarsAsync("NOPE", Arg.Any<CancellationToken>())
            .ThrowsAsync(new SymbolNotFoundException("NOPE"));

        HttpResponseMessage response =
            await _factory.CreateClient().GetAsync("/api/stocks/NOPE/intraday");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");

        using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("status").GetInt32().ShouldBe(404);
    }

    [Fact]
    public async Task Maps_UpstreamException_to_a_502_that_leaks_nothing()
    {
        _provider.GetIntradayBarsAsync("TSLA", Arg.Any<CancellationToken>())
            .ThrowsAsync(new UpstreamException("Yahoo Finance returned HTTP 503 for 'TSLA'."));

        HttpResponseMessage response =
            await _factory.CreateClient().GetAsync("/api/stocks/TSLA/intraday");

        response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");

        string body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain("Yahoo");
        body.ShouldNotContain("503");

        using JsonDocument doc = JsonDocument.Parse(body);
        bool hasDetail = doc.RootElement.TryGetProperty("detail", out JsonElement detail)
            && detail.ValueKind is not JsonValueKind.Null;
        hasDetail.ShouldBeFalse();
    }
}
