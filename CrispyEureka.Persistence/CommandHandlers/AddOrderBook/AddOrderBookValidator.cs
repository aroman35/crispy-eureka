using System.Linq;
using CrispyEureka.Application.Commands.MarketData;
using CrispyEureka.Domain.MarketData.OrderBook;
using FluentValidation;

namespace CrispyEureka.Persistence.CommandHandlers.AddOrderBook
{
    public class AddOrderBookValidator : AbstractValidator<AddMarketData<OrderBook>>
    {
        public AddOrderBookValidator()
        {
            RuleFor(x => x.Figi).NotNull().NotEmpty();
            RuleFor(x => x.Messages.Any()).Must(_ => true);
            RuleFor(x => x.Messages.Select(orderBook => orderBook.Figi).Distinct().Count()).Equal(1);
            RuleFor(x => x.Messages.Select(orderBook => orderBook.Figi).Distinct().First()).Equal(x => x.Figi);
        }
    }
}