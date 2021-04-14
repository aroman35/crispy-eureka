using System.Linq;
using FluentValidation;

namespace CrispyEureka.Persistence.CommandHandlers.AddOrderBook
{
    public class AddOrderBookValidator : AbstractValidator<CrispyEureka.Application.Commands.MarketData.OrderBook.AddOrderBookCommand>
    {
        public AddOrderBookValidator()
        {
            RuleFor(x => x.Figi).NotNull().NotEmpty();
            RuleFor(x => x.OrderBooks.Any()).Must(_ => true);
            RuleFor(x => x.OrderBooks.Select(orderBook => orderBook.Figi).Distinct().Count()).Equal(1);
            RuleFor(x => x.OrderBooks.Select(orderBook => orderBook.Figi).Distinct().First()).Equal(x => x.Figi);
        }
    }
}