using System.Collections.Generic;
using MediatR;

namespace CrispyEureka.Application.Commands.MarketData.OrderBook
{
    public class AddOrderBookCommand : IRequest
    {
        public AddOrderBookCommand(
            string figi,
            IEnumerable<CrispyEureka.Domain.MarketData.OrderBook.OrderBook> orderBooks)
        {
            Figi = figi;
            OrderBooks = orderBooks;
        }

        public string Figi { get; }
        public IEnumerable<CrispyEureka.Domain.MarketData.OrderBook.OrderBook> OrderBooks { get; }
    }
}