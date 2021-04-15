using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyEureka.Transfer
{
    public interface IEurekaConsumer<TMessagePayload> : IDisposable
        where TMessagePayload : IMessagePayload
    {
        Task Consume(CancellationToken cancellationToken);
    }
}