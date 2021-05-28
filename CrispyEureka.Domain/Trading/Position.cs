using System.Collections.Generic;
using System.Linq;

namespace CrispyEureka.Domain.Trading
{
    public class Position
    {
        private Position()
        {
            Orders = new HashSet<Order>();
        }

        public Position(string accountId, string figi, Currency currency) : this()
        {
            AccountId = accountId;
            Figi = figi;
            Currency = currency;
        }

        public Position(string accountId, string figi, Currency currency, Order order) : this(accountId, figi, currency)
        {
            Orders.Add(order);
        }
        public string AccountId { get; }
        public string Figi { get; }
        public Currency Currency { get; }
        public ICollection<Order> Orders { get; }
        private IEnumerable<Order> ExecutedBuyOrders => Orders.Where(x =>
            x.Operation == OperationType.Buy && (x.Status is OrderStatus.Fill or OrderStatus.PartiallyFill));
        
        private IEnumerable<Order> ExecutedSellOrders => Orders.Where(x =>
            x.Operation == OperationType.Sell && (x.Status is OrderStatus.Fill or OrderStatus.PartiallyFill));
        
        public int TotalLots => ExecutedBuyOrders.Sum(x => x.ExecutedLots) - ExecutedSellOrders.Sum(x => x.ExecutedLots);

        public decimal? Price => IsOpened ? ExecutedBuyOrders.Sum(x => x.Price * x.ExecutedLots) - ExecutedSellOrders.Sum(x => x.Price * x.ExecutedLots) : null;

        public decimal? Average => IsOpened ? Price / TotalLots : null;
        
        public bool IsOpened =>
            ExecutedBuyOrders.Sum(x => x.ExecutedLots) !=
            ExecutedSellOrders.Sum(x => x.ExecutedLots);

        public bool IsLong => ExecutedBuyOrders.Sum(x => x.ExecutedLots) >= ExecutedSellOrders.Sum(x => x.ExecutedLots);

        public bool IsShort => !IsLong;

        public decimal? NotRealisedPnl(decimal marketClose) => IsOpened ? marketClose * TotalLots - Price!.Value : null;

        public decimal RealisedPnl => IsLong
            ? ExecutedSellOrders.Sum(x => x.ExecutedLots * x.Price) - ExecutedBuyOrders.Sum(x => x.ExecutedLots * x.Price)
            : (ExecutedSellOrders.Sum(x => x.ExecutedLots * x.Price) - ExecutedBuyOrders.Sum(x => x.ExecutedLots * x.Price)) * -1;

        public Position Add(Order order)
        {
            Orders.Add(order);
            return this;
        }
    }
}