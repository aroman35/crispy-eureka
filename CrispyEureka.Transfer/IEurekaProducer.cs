using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyEureka.Transfer
{
    public interface IEurekaProducer<TMessagePayload> : IDisposable
        where TMessagePayload : IMessagePayload
    {
        Task InitializeExchange();
        Task StartProducingJob(CancellationToken cancellationToken);
    }
}