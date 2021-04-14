namespace CrispyEureka.Domain.MarketData.OrderBook
{
    public class OrderBookEntry
    {
        public OrderBookEntry(decimal price, int quantity)
        {
            Price = price;
            Quantity = quantity;
        }
        
        public OrderBookEntry(decimal[] raw)
        {
            Price = raw[0];
            Quantity = (int)raw[1];
        }
        public decimal Price { get; }
        public int Quantity { get; }
    }
}