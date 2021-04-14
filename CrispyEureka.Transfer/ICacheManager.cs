using System;
using System.Collections.Generic;

namespace CrispyEureka.Transfer
{
    public interface ICacheManager<TMessagePayload> : IDisposable
        where TMessagePayload : IMessagePayload
    {
        void AddMessage(TMessagePayload message);
        IAsyncEnumerable<TransferMessage<TMessagePayload>> ReadBatch(int batchSize);
        void ConfirmDelivery(string id);
    }
}