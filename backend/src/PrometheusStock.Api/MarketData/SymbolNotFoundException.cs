namespace PrometheusStock.Api.MarketData;

/// <summary>
/// Thrown by an <see cref="IStockDataProvider" /> when the upstream source has no
/// data for the requested symbol. The global exception handler maps this to
/// HTTP 404; genuine upstream failures use a different exception and map to 502.
/// </summary>
public sealed class SymbolNotFoundException(string symbol)
    : Exception($"No market data found for symbol '{symbol}'.")
{
    /// <summary>The symbol that could not be resolved.</summary>
    public string Symbol { get; } = symbol;
}
