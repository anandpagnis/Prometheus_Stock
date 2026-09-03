namespace PrometheusStock.Api.MarketData;

/// <summary>
/// Fetches raw intraday bars for a symbol from an external market-data source.
/// Implementations own the transport, the wire-format mapping and the translation
/// of provider-specific errors into domain exceptions. Everything above this seam
/// works in terms of <see cref="IntradayBar"/> and never sees the provider's types.
/// </summary>
public interface IStockDataProvider
{
    /// <param name="symbol">Ticker symbol. Callers validate format before calling.</param>
    /// <param name="cancellationToken">Cancels the outbound request.</param>
    /// <returns>
    /// Intraday bars covering the provider's configured look-back window, expressed
    /// in the exchange time zone. Order is not guaranteed.
    /// </returns>
    /// <exception cref="SymbolNotFoundException">
    /// The source has no data for <paramref name="symbol" />.
    /// </exception>
    /// <exception cref="UpstreamException">
    /// The source failed: a non-success status, an unparseable body, or retries exhausted.
    /// </exception>
    Task<IReadOnlyList<IntradayBar>> GetIntradayBarsAsync(string symbol, CancellationToken cancellationToken);
}
