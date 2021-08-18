using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrispyEureka.Crypto.OrderBook.Producer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = new BinanceClient();

            var tickersResponse = await client.Spot.Market.GetTickersAsync(stoppingToken);
            var tickers = tickersResponse.Data
                .Where(x => x.TotalTrades > 1000)
                .Select(x => x.Symbol)
                .Distinct()
                .Take(1024)
                .ToArray();
            
            var binanceSocketClient = new BinanceSocketClient(new BinanceSocketClientOptions
            {
                AutoReconnect = true,
                SocketResponseTimeout = TimeSpan.FromMinutes(1)
            });
            var receivedSymbols = new HashSet<string>();
            var subscription = await binanceSocketClient.Spot.SubscribeToPartialOrderBookUpdatesAsync(tickers, 5, 1000, book =>
            {
                var bestBid = book.Data.Bids.OrderBy(x => x.Price).FirstOrDefault();
                var bestAsk = book.Data.Asks.OrderByDescending(x => x.Price).FirstOrDefault();
                if (!receivedSymbols.TryGetValue(book.Topic, out _))
                    receivedSymbols.Add(book.Topic);

                _logger.LogInformation("{Time}[{Symbol}]\tBid: {BidPrice} ({BidVol})\tAsk: {AskPrice} ({AskVol})\tBid avg: {BidAvg}[{BidAvgVol}]\tAsk avg: {AskAvg}[{AskAvgVol}]",
                    book.Timestamp,
                    book.Data.Symbol,
                    bestBid?.Price,
                    bestBid?.Quantity,
                    bestAsk?.Price,
                    bestAsk?.Quantity,
                    book.Data.Bids.Average(x => x.Price),
                    book.Data.Bids.Sum(x => x.Quantity),
                    book.Data.Asks.Average(x => x.Price),
                    book.Data.Asks.Sum(x => x.Quantity)
                );
            });

            await Task.Delay(10000, stoppingToken);
            await subscription.Data.CloseAsync();
            _logger.LogInformation("Total symbols {total} / {tickers}", receivedSymbols.Count, tickers.Length);
            _lifetime.StopApplication();
        }
    }
}