namespace PrometheusStock.Api.MarketData;

/// <summary>
/// Thrown by an <see cref="IStockDataProvider" /> when the upstream market-data
/// source fails in a way the caller cannot fix: a non-success status that is not a
/// missing symbol, a body that cannot be parsed, or retries exhausted against a
/// failing endpoint. The global exception handler maps this to HTTP 502.
/// </summary>
public sealed class UpstreamException(string message, Exception? innerException = null)
    : Exception(message, innerException);
