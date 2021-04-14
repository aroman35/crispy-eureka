﻿using System;
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

        public async Task OrderBookSubscribe(string figi, int depth = 20)
        {
            await _context.SendStreamingRequestAsync(new StreamingRequest.OrderbookSubscribeRequest(figi, depth));
            _context.StreamingEventReceived += OrderBookEventReceived;
        }

        public async Task CandlesSubscribe(string figi)
        {
            await _context.SendStreamingRequestAsync(new StreamingRequest.CandleSubscribeRequest(figi, CandleInterval.Minute));
            _context.StreamingEventReceived += CandleEventReceived;
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