namespace PrometheusStock.Api.MarketData;

/// <inheritdoc cref="IIntradayAggregator" />
public sealed class IntradayAggregator : IIntradayAggregator
{
    /// <inheritdoc />
    public IReadOnlyList<DailyAggregate> Aggregate(IReadOnlyList<IntradayBar> bars)
    {
        // Group on the bar's own local calendar date: its offset already reflects the
        // exchange time zone, so no conversion happens here.
        return bars
            .GroupBy(bar => DateOnly.FromDateTime(bar.Timestamp.Date))
            .OrderBy(group => group.Key)
            .Select(group => new DailyAggregate(
                Day: group.Key,
                LowAverage: group.Average(bar => bar.Low),
                HighAverage: group.Average(bar => bar.High),
                Volume: group.Sum(bar => bar.Volume)))
            .ToList();
    }
}
