using System;

namespace CrispyEureka.Domain.Trading
{
    public class Order
    {
        public string ExchangeOrderId { get; set; }
        public Guid ClientOrderId { get; set; }
        
        public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
        public OperationType Operation { get; set; }
        public decimal Price { get; set; }
        public int RequestedLots { get; set; }
        public int ExecutedLots { get; set; }
        public bool IsFullFilled => RequestedLots == ExecutedLots;

        public override string ToString()
        {
            return Operation == OperationType.Buy
                ? $"{Operation.ToString()}\t\t{ExecutedLots}/{RequestedLots} x {Price}\t{Status.ToString()}"
                : $"{Operation.ToString()}\t{ExecutedLots}/{RequestedLots} x {Price}\t{Status.ToString()}";
        }
    }
}