using Microsoft.Extensions.Options;
using PrometheusStock.Api.MarketData;
using PrometheusStock.Api.MarketData.Yahoo;

namespace PrometheusStock.Api.Extensions;

/// <summary>
/// Composition root for the market-data slice: the pure <see cref="IIntradayAggregator" />,
/// the Yahoo-backed <see cref="IStockDataProvider" /> as a typed <see cref="HttpClient" />,
/// and the validated <see cref="YahooFinanceOptions" />.
/// </summary>
public static class MarketDataServiceCollectionExtensions
{
    public static IServiceCollection AddMarketData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<YahooFinanceOptions>()
            .Bind(configuration.GetSection(YahooFinanceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IIntradayAggregator, IntradayAggregator>();

        services.AddHttpClient<IStockDataProvider, YahooFinanceClient>(static (serviceProvider, httpClient) =>
        {
            YahooFinanceOptions options =
                serviceProvider.GetRequiredService<IOptions<YahooFinanceOptions>>().Value;

            httpClient.BaseAddress = new Uri(options.BaseUrl);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        return services;
    }
}
