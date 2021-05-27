using System;
using CrispyEureka.Domain.Trading;

namespace CrispyEureka.Testing.Domain
{
    public static class Mocks
    {
        public static string Figi = "TSLA";
        public static string Account = "test-account";
        public static Currency DefaultCurrency = Currency.USD;

        public static Position EmptyPosition() => new(Account, Figi, DefaultCurrency);

        public static Order CreateLongOrder(decimal price, int lots) => new()
        {
            ExchangeOrderId = "test",
            ClientOrderId = Guid.NewGuid(),
            Type = OrderType.Limit,
            Status = OrderStatus.New,
            Operation = OperationType.Buy,
            Price = price,
            RequestedLots = lots
        };
        
        public static Order CreateShortOrder(decimal price, int lots) => new()
        {
            ExchangeOrderId = "test",
            ClientOrderId = Guid.NewGuid(),
            Type = OrderType.Limit,
            Status = OrderStatus.New,
            Operation = OperationType.Sell,
            Price = price,
            RequestedLots = lots
        };

        public static Order CloseFullFill(Order requestOrder) => new()
        {
            ExchangeOrderId = requestOrder.ExchangeOrderId,
            ClientOrderId = Guid.NewGuid(),
            Type = requestOrder.Type,
            Status = OrderStatus.Fill,
            Operation = requestOrder.Operation,
            Price = requestOrder.Price,
            RequestedLots = requestOrder.RequestedLots,
            ExecutedLots = requestOrder.RequestedLots
        };

        public static Order ClosePartial(Order requestOrder, int lots) => new()
        {
            ExchangeOrderId = requestOrder.ExchangeOrderId,
            ClientOrderId = Guid.NewGuid(),
            Type = requestOrder.Type,
            Status = OrderStatus.PartiallyFill,
            Operation = requestOrder.Operation,
            Price = requestOrder.Price,
            RequestedLots = requestOrder.RequestedLots,
            ExecutedLots = lots
        };
    }
}