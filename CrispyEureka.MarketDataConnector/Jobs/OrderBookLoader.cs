using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CrispyEureka.MarketDataConnector.TinkoffConnector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrispyEureka.MarketDataConnector.Jobs
{
    public class OrderBookLoader : BackgroundService
    {
        private readonly TinkoffConnector.TinkoffConnector _connector;
        private readonly ILogger<OrderBookLoader> _logger;
        private readonly MarketDataLoadSettings _dataLoadSettings;

        public OrderBookLoader(
            TinkoffConnector.TinkoffConnector connector,
            ILogger<OrderBookLoader> logger,
            MarketDataLoadSettings dataLoadSettings)
        {
            _connector = connector;
            _logger = logger;
            _dataLoadSettings = dataLoadSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var figiesList = await GetFigies(2000, stoppingToken).ToArrayAsync(stoppingToken);
            foreach (var figi in figiesList)
            {
                await _connector.OrderBookSubscribe(figi, 20);
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10_000, stoppingToken);
                _logger.LogInformation("OrderBook load job is in progress");
            }
        }

        private async IAsyncEnumerable<string> GetFigies(int timeoutMs, [EnumeratorCancellation] CancellationToken stoppingToken)
        {
            foreach (var ticker in _dataLoadSettings.Tickers)
            {
                var figi = await _connector.GetFigiByTicker(ticker);
                yield return figi;
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}