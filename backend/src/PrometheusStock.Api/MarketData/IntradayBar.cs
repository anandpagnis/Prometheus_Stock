namespace PrometheusStock.Api.MarketData;

/// <summary>
/// A single intraday price bar (one interval, e.g. 15 minutes) for a symbol,
/// normalised to the exchange's local time zone. Provider-agnostic: no field here
/// is specific to Yahoo or any other source.
/// </summary>
/// <param name="Timestamp">Start of the interval, in the exchange time zone.</param>
/// <param name="High">Highest traded price during the interval.</param>
/// <param name="Low">Lowest traded price during the interval.</param>
/// <param name="Volume">Shares traded during the interval.</param>
public sealed record IntradayBar(
    DateTimeOffset Timestamp,
    decimal High,
    decimal Low,
    long Volume);
