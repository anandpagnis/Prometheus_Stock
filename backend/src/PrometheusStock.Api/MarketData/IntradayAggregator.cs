namespace PrometheusStock.Api.MarketData;

/// <inheritdoc cref="IIntradayAggregator" />
public sealed class IntradayAggregator : IIntradayAggregator
{
    /// <inheritdoc />
    public IReadOnlyList<DailyAggregate> Aggregate(IReadOnlyList<IntradayBar> bars) =>
        throw new NotImplementedException();
}
