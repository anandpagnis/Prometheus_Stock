using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PrometheusStock.Api.Extensions;
using PrometheusStock.Api.MarketData;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PrometheusStock.Tests.Integration;

/// <summary>
/// Exercises <c>YahooFinanceClient</c> end to end: a real <c>AddMarketData</c> container
/// with <c>YahooFinance:BaseUrl</c> pointed at a WireMock stand-in for Yahoo, so the
/// typed <see cref="System.Net.Http.HttpClient"/> and the real DI wiring are in the loop.
/// The MVP client has no retry layer, so every call is expected to hit the upstream
/// exactly once. Written test-first — the client is still a stub, so every case here is
/// red until it maps payloads and translates failures.
/// </summary>
public sealed class YahooFinanceClientTests : IDisposable
{
    private const string Symbol = "TSLA";
    private const string ChartPath = "/v8/finance/chart/TSLA";
    private const string TestUserAgent = "prometheus-stock-tests/1.0";

    /// <summary>Real TSLA capture (5d, 15m): 117 bars, quote indices 5, 6 and 40 nulled.</summary>
    private static readonly string FixturePath =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "yahoo-chart-5d.json");

    /// <summary>
    /// Hand-authored: 5 bars straddling the 02 Nov 2025 America/New_York EDT→EST change.
    /// <c>meta.gmtoffset</c> is deliberately the EDT value for every bar, so a client that
    /// trusts the scalar rather than converting each timestamp in the exchange zone fails.
    /// </summary>
    private static readonly string DstFixturePath =
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "yahoo-chart-dst.json");

    /// <summary>Yahoo's "unknown symbol" envelope: HTTP 200 but a null result plus an error node.</summary>
    private const string NullResultEnvelope =
        """{"chart":{"result":null,"error":{"code":"Not Found","description":"No data found, symbol may be delisted"}}}""";

    private readonly WireMockServer _yahoo = WireMockServer.Start();
    private readonly List<ServiceProvider> _containers = [];

    public void Dispose()
    {
        foreach (ServiceProvider container in _containers)
        {
            container.Dispose();
        }

        _yahoo.Dispose();
    }

    // -- payload mapping --------------------------------------------------------

    [Fact]
    public async Task Maps_chart_payload_to_intraday_bars()
    {
        StubChart(status: 200, body: await File.ReadAllTextAsync(FixturePath));
        IStockDataProvider provider = CreateProvider();

        IReadOnlyList<IntradayBar> bars = await provider.GetIntradayBarsAsync(Symbol, CancellationToken.None);

        bars.Count.ShouldBe(114); // 117 timestamps minus the three nulled quote rows
        bars.ShouldAllBe(bar => bar.High > 0m && bar.Low > 0m && bar.High >= bar.Low);
        bars.ShouldAllBe(bar => bar.Timestamp.Offset == TimeSpan.FromSeconds(-14400));
        bars.Select(bar => bar.Timestamp).ShouldBeInOrder(SortDirection.Ascending);
    }

    [Fact]
    public async Task Each_timestamp_is_converted_in_the_exchange_zone_across_a_dst_change()
    {
        StubChart(status: 200, body: await File.ReadAllTextAsync(DstFixturePath));
        IStockDataProvider provider = CreateProvider();

        IntradayBar[] bars =
        [
            .. (await provider.GetIntradayBarsAsync(Symbol, CancellationToken.None))
                .OrderBy(bar => bar.Timestamp),
        ];

        bars.Length.ShouldBe(5);

        TimeSpan edt = TimeSpan.FromHours(-4);
        TimeSpan est = TimeSpan.FromHours(-5);
        bars[0].Timestamp.Offset.ShouldBe(edt); // 03:45 UTC — before the 06:00 UTC change
        bars[1].Timestamp.Offset.ShouldBe(edt); // 05:45 UTC
        bars[2].Timestamp.Offset.ShouldBe(est); // 06:15 UTC — after the change
        bars[3].Timestamp.Offset.ShouldBe(est); // 06:30 UTC
        bars[4].Timestamp.Offset.ShouldBe(est); // 07:00 UTC

        // bar 0 is 23:45 EDT on 01 Nov, so its local calendar day is the 1st, not the 2nd.
        DateOnly.FromDateTime(bars[0].Timestamp.Date).ShouldBe(new DateOnly(2025, 11, 1));
    }

    [Fact]
    public async Task Sends_configured_range_interval_and_user_agent()
    {
        StubChart(status: 200, body: await File.ReadAllTextAsync(FixturePath));
        IStockDataProvider provider = CreateProvider(
            ("YahooFinance:Range", "3mo"),
            ("YahooFinance:Interval", "30m"));

        await provider.GetIntradayBarsAsync(Symbol, CancellationToken.None);

        ChartRequestCount.ShouldBe(1);
        var request = _yahoo.LogEntries.Single().RequestMessage!;
        request.Path.ShouldBe(ChartPath);
        request.Query!["range"].ShouldContain("3mo");
        request.Query!["interval"].ShouldContain("30m");
        request.Headers!["User-Agent"].ShouldContain(TestUserAgent);
    }

    // -- missing symbol -------------------------------------------------------------

    [Fact]
    public async Task Raw_404_is_translated_to_SymbolNotFoundException()
    {
        StubChart(status: 404, body: "Not Found", contentType: "text/plain");
        IStockDataProvider provider = CreateProvider();

        var ex = await Should.ThrowAsync<SymbolNotFoundException>(
            () => provider.GetIntradayBarsAsync(Symbol, CancellationToken.None));

        ex.Symbol.ShouldBe(Symbol);
        ChartRequestCount.ShouldBe(1); // a 404 is surfaced, never retried
    }

    [Fact]
    public async Task Null_result_with_error_node_is_translated_to_SymbolNotFoundException()
    {
        StubChart(status: 200, body: NullResultEnvelope);
        IStockDataProvider provider = CreateProvider();

        var ex = await Should.ThrowAsync<SymbolNotFoundException>(
            () => provider.GetIntradayBarsAsync(Symbol, CancellationToken.None));

        ex.Symbol.ShouldBe(Symbol);
    }

    // -- upstream failure ---------------------------------------------------------

    [Fact]
    public async Task A_500_is_translated_to_UpstreamException_after_a_single_request()
    {
        StubChart(status: 500, body: "server error", contentType: "text/plain");
        IStockDataProvider provider = CreateProvider();

        await Should.ThrowAsync<UpstreamException>(
            () => provider.GetIntradayBarsAsync(Symbol, CancellationToken.None));

        ChartRequestCount.ShouldBe(1); // no retry layer — the failure is surfaced immediately
    }

    [Fact]
    public async Task Unparseable_body_is_translated_to_UpstreamException()
    {
        StubChart(status: 200, body: "<html><body>definitely not json</body></html>");
        IStockDataProvider provider = CreateProvider();

        await Should.ThrowAsync<UpstreamException>(
            () => provider.GetIntradayBarsAsync(Symbol, CancellationToken.None));
    }

    // -- fixture ----------------------------------------------------------------

    private IStockDataProvider CreateProvider(params (string Key, string Value)[] overrides)
    {
        Dictionary<string, string?> settings = new()
        {
            ["YahooFinance:BaseUrl"] = _yahoo.Url,
            ["YahooFinance:UserAgent"] = TestUserAgent,
            ["YahooFinance:Range"] = "1mo",
            ["YahooFinance:Interval"] = "15m",
        };

        foreach ((string key, string value) in overrides)
        {
            settings[key] = value;
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        ServiceProvider container = new ServiceCollection()
            .AddMarketData(configuration)
            .BuildServiceProvider();

        _containers.Add(container);
        return container.GetRequiredService<IStockDataProvider>();
    }

    private void StubChart(int status, string body, string contentType = "application/json") =>
        _yahoo
            .Given(Request.Create().WithPath(ChartPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(status)
                .WithHeader("Content-Type", contentType)
                .WithBody(body));

    private int ChartRequestCount =>
        _yahoo.LogEntries.Count(entry =>
            entry.RequestMessage is { Method: "GET", Path: ChartPath });
}
