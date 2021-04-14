using System.Collections.Generic;
using MediatR;
using CrispyEureka.Domain.MarketData.Candle;

namespace CrispyEureka.Application.Commands.MarketData.Candles
{
    public class AddCandlesCommand : IRequest
    {
        public AddCandlesCommand(string figi, IEnumerable<Candle> candles)
        {
            Figi = figi;
            Candles = candles;
        }

        public string Figi { get; }
        public IEnumerable<Candle> Candles { get; }
    }
}