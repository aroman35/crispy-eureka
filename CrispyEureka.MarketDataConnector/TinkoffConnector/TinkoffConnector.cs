using System;
using System.Linq;
using System.Threading.Tasks;
using CrispyEureka.Transfer;
using CrispyEureka.Transfer.Models;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace CrispyEureka.MarketDataConnector.TinkoffConnector
{
    public class TinkoffConnector : IDisposable
    {
        private readonly Connection _connection;
        private readonly Context _context;
        private readonly ICacheManager<OrderBookTransferModel> _orderBookCache;
        private readonly ICacheManager<CandleTransferModel> _candlesCache;
        private readonly ILogger<TinkoffConnector> _logger;

        public TinkoffConnector(
            TinkoffSettings settings,
            ILogger<TinkoffConnector> logger,
            ICacheManager<OrderBookTransferModel> orderBookCache,
            ICacheManager<CandleTransferModel> candlesCache)
        {
            _logger = logger;
            _orderBookCache = orderBookCache;
            _candlesCache = candlesCache;
            _connection = ConnectionFactory.GetConnection(settings.ApiKey);
            _context = _connection.Context;
        }

        public async Task<string> GetFigiByTicker(string ticker)
        {
            var searchList = await _context.MarketSearchByTickerAsync(ticker);
            
            var instrument = searchList.Instruments.First();
            return instrument.Figi;
        }
        
        public async Task OrderBookSubscribe(string figi, int depth = 20)
        {
            var orders = await _context.OrdersAsync();
            var order = orders.First();
            await _context.SendStreamingRequestAsync(new StreamingRequest.OrderbookSubscribeRequest(figi, depth));
            _context.StreamingEventReceived += OrderBookEventReceived;
        }

        public async Task CandlesSubscribe(string figi)
        {
            await _context.SendStreamingRequestAsync(new StreamingRequest.CandleSubscribeRequest(figi, CandleInterval.Minute));
            _context.StreamingEventReceived += CandleEventReceived;
        }

        public async Task LoadTodayCandles(string figi)
        {
            var candleList = await _context.MarketCandlesAsync(figi, DateTime.UtcNow.Date, DateTime.UtcNow, CandleInterval.Minute);
            foreach (var candlePayload in candleList.Candles)
            {
                var candle = new CandleTransferModel
                {
                    Figi = candlePayload.Figi,
                    Timestamp = candlePayload.Time.Ticks,
                    Interval = (int) candlePayload.Interval,
                    Open = candlePayload.Open,
                    Close = candlePayload.Close,
                    High = candlePayload.High,
                    Low = candlePayload.Low,
                    Volume = candlePayload.Volume
                };
                
                _candlesCache.AddMessage(candle);
                _logger.LogInformation($"Sent candle {candle.Figi}:{candlePayload.Time:s}");
            }
        }

        private void OrderBookEventReceived(object sender, StreamingEventReceivedEventArgs e)
        {
            if (e.Response is not OrderbookResponse orderBookResponse) return;
            var orderBook = new OrderBookTransferModel
            {
                Figi = orderBookResponse.Payload.Figi,
                Timestamp = orderBookResponse.Time.Ticks,
                Asks = orderBookResponse.Payload.Asks.Select(x => new OrderBookEntry
                {
                    Price = x[0],
                    Quantity = (int) x[1]
                }).ToArray(),
                Bids = orderBookResponse.Payload.Bids.Select(x => new OrderBookEntry
                {
                    Price = x[0],
                    Quantity = (int) x[1]
                }).ToArray()
            };
            _orderBookCache.AddMessage(orderBook);
        }

        private void CandleEventReceived(object sender, StreamingEventReceivedEventArgs e)
        {
            if (e.Response is not CandleResponse candleResponse) return;
            var candle = new CandleTransferModel
            {
                Figi = candleResponse.Payload.Figi,
                Timestamp = candleResponse.Payload.Time.Ticks,
                Interval = (int) candleResponse.Payload.Interval,
                Open = candleResponse.Payload.Open,
                Close = candleResponse.Payload.Close,
                High = candleResponse.Payload.High,
                Low = candleResponse.Payload.Low,
                Volume = candleResponse.Payload.Volume
            };
                
            _candlesCache.AddMessage(candle);
        }

        public void Dispose()
        {
            _connection.Dispose();
            _context.Dispose();
        }
    }
}