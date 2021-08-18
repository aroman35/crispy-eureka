using System;
using System.Threading;
using System.Threading.Tasks;
using CrispyEureka.Domain.Trading;
using MediatR;

namespace CrispyEureka.Core.UpDownStrategy.CalculateProfitClose
{
    public record CalculateProfitCloseCommand(Position OpenedPosition, decimal LastClose) : IRequest<Order>;

    public class CalculateProfitCloseHandler : IRequestHandler<CalculateProfitCloseCommand, Order>
    {
        private readonly UpDownSettings _upDownSettings;

        public CalculateProfitCloseHandler(UpDownSettings upDownSettings)
        {
            _upDownSettings = upDownSettings;
        }

        public Task<Order> Handle(CalculateProfitCloseCommand request, CancellationToken cancellationToken)
        {
            if (!request.OpenedPosition.IsOpened)
                throw new ArgumentException("Position is closed");

            var notRealisedPnl = request.OpenedPosition.NotRealisedPnl(request.LastClose);

            var order = new Order
            {
                Operation = request.OpenedPosition.IsLong ? OperationType.Sell : OperationType.Buy,

            };
            return Task.FromResult(order);
        }

        private OperationType GetReverseType(OperationType sourceType)
        {
            return sourceType switch
            {
                OperationType.Buy => OperationType.Sell,
                OperationType.Sell => OperationType.Buy,
                _ => throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null)
            };
        }
    }
}