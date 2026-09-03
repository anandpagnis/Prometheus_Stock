using Microsoft.Extensions.Options;

namespace PrometheusStock.Api.MarketData.Yahoo;

/// <summary>
/// <see cref="IStockDataProvider" /> backed by Yahoo Finance's
/// <c>v8/finance/chart</c> endpoint. Registered as a typed <see cref="HttpClient" />
/// in <c>AddMarketData</c>, where the base address, timeout and User-Agent are
/// configured from <see cref="YahooFinanceOptions" />.
/// </summary>
public sealed class YahooFinanceClient(HttpClient httpClient, IOptions<YahooFinanceOptions> options)
    : IStockDataProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly YahooFinanceOptions _options = options.Value;

    /// <inheritdoc />
    public Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(string symbol, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        // TODO(intraday): send
        //   GET v8/finance/chart/{symbol}?interval={_options.Interval}&range={_options.Range}
        // with _httpClient, map chart.result[0] (timestamps + quote high/low/volume) to
        // IntradayBar[] in the exchange time zone, and throw SymbolNotFoundException when
        // Yahoo returns 404 or a null result node.
        _ = (_httpClient, _options);
        throw new NotImplementedException();
    }
}
