using System.Linq;
using CrispyEureka.Application.Commands.MarketData.Candles;
using FluentValidation;

namespace CrispyEureka.Persistence.CommandHandlers.AddCandles
{
    public class AddCandlesValidator : AbstractValidator<AddCandlesCommand>
    {
        public AddCandlesValidator()
        {
            RuleFor(x => x.Figi).NotNull().NotEmpty();
            RuleFor(x => x.Candles.Any()).Must(_ => true);
            RuleFor(x => x.Candles.Distinct().Count()).Equal(x => x.Candles.Count());
        }
    }
}