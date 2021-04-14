using System;
using System.Collections.Generic;
using System.Linq;

namespace CrispyEureka.Domain.MarketData.OrderBook
{
    public class OrderBook : AggregateRoot
    {
        public OrderBook(DateTime timestamp,
            IEnumerable<OrderBookEntry> bids,
            IEnumerable<OrderBookEntry> asks,
            string figi)
        {
            Timestamp = timestamp;
            _bidsRaw = bids;
            _asksRaw = asks;
            
            Depth = Math.Max(_bidsRaw.Count(), _asksRaw.Count());
            Figi = figi;
        }
        public OrderBook(
            DateTime timestamp,
            IEnumerable<decimal[]> bids,
            IEnumerable<decimal[]> asks,
            string figi)
        {
            Timestamp = timestamp;
            _bidsRaw = bids.Select(x => new OrderBookEntry(x));
            _asksRaw = asks.Select(x => new OrderBookEntry(x));
            
            Depth = Math.Max(_bidsRaw.Count(), _asksRaw.Count());
            Figi = figi;
        }

        private readonly IEnumerable<OrderBookEntry> _bidsRaw;
        private readonly IEnumerable<OrderBookEntry> _asksRaw;

        public int Depth { get; }
        public string Figi { get; }
        public DateTime Timestamp { get; }
        public OrderBookEntry[] Asks => _asksRaw.OrderBy(x => x.Price).ToArray();
        public OrderBookEntry[] Bids => _bidsRaw.OrderByDescending(x => x.Price).ToArray();
        public OrderBookEntry BestAsk => Asks[0];
        public OrderBookEntry BestBid => Bids[0];
    }
}