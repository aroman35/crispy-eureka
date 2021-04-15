using System.Collections.Generic;
using CrispyEureka.Domain;
using MediatR;

namespace CrispyEureka.Application.Commands.MarketData
{
    public class AddMarketData<TMarketDataType> : IRequest
        where TMarketDataType : AggregateRoot
    {
        public AddMarketData(IEnumerable<TMarketDataType> messages, string figi)
        {
            Messages = messages;
            Figi = figi;
        }

        public IEnumerable<TMarketDataType> Messages { get; }
        public string Figi { get; }
    }
}