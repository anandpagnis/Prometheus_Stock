namespace PrometheusStock.Api.MarketData;

/// <summary>
/// Per-day roll-up of intraday bars for one exchange-local calendar day: the mean
/// of the interval lows and highs and the total traded volume. Carries full
/// precision — rounding to the API's fixed 4 decimal places happens only at the
/// response boundary, never here.
/// </summary>
/// <param name="Day">The exchange-local calendar day these figures cover.</param>
/// <param name="LowAverage">Mean of every interval <see cref="IntradayBar.Low"/> that day.</param>
/// <param name="HighAverage">Mean of every interval <see cref="IntradayBar.High"/> that day.</param>
/// <param name="Volume">Sum of every interval <see cref="IntradayBar.Volume"/> that day.</param>
public sealed record DailyAggregate(
    DateOnly Day,
    decimal LowAverage,
    decimal HighAverage,
    long Volume);
