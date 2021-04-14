using System;
using System.Collections.Generic;
using System.Linq;
using CrispyEureka.Domain.MarketData.OrderBook;

namespace CrispyEureka.Persistence.Models
{
    public class OrderBookDto : IDto<OrderBook>
    {
        public OrderBookDto()
        {
            Id = Guid.NewGuid();
            Bids = new List<OrderBookEntryDto>();
            Asks = new List<OrderBookEntryDto>();
        }
        
        public Guid Id { get; private set; }
        public OrderBook ToRoot()
        {
            var orderBook = new OrderBook(
                Timestamp,
                Bids.Select(x => new OrderBookEntry(x.Price, x.Quantity)),
                Asks.Select(x => new OrderBookEntry(x.Price, x.Quantity)),
                Figi);

            return orderBook;
        }

        public DateTime Timestamp { get; set; }
        public string Figi { get; set; }
        public ICollection<OrderBookEntryDto> Bids { get; private set; }
        public ICollection<OrderBookEntryDto> Asks { get; private set; }
    }

    public class OrderBookEntryDto
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}