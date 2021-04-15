using System.Linq;
using CrispyEureka.Application.Commands.MarketData;
using CrispyEureka.Application.Commands.MarketData.Candles;
using CrispyEureka.Domain.MarketData.Candle;
using FluentValidation;

namespace CrispyEureka.Persistence.CommandHandlers.AddCandles
{
    public class AddCandlesValidator : AbstractValidator<AddMarketData<Candle>>
    {
        public AddCandlesValidator()
        {
            RuleFor(x => x.Figi).NotNull().NotEmpty();
            RuleFor(x => x.Messages.Any()).Must(_ => true);
            RuleFor(x => x.Messages.Distinct().Count()).Equal(x => x.Messages.Count());
        }
    }
}