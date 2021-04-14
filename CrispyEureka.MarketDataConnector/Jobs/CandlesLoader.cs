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
    public class CandlesLoader : BackgroundService
    {
        private readonly TinkoffConnector.TinkoffConnector _connector;
        private readonly ILogger<CandlesLoader> _logger;
        private readonly MarketDataLoadSettings _dataLoadSettings;

        public CandlesLoader(TinkoffConnector.TinkoffConnector connector, ILogger<CandlesLoader> logger, MarketDataLoadSettings dataLoadSettings)
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
                await _connector.CandlesSubscribe(figi);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10_000, stoppingToken);
                _logger.LogInformation("Candles load job is in progress");
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