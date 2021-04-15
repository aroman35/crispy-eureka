using System;
using System.Collections.Generic;
using System.Linq;
using CrispyEureka.Transfer;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;

namespace CrispyEureka.MarketDataConnector.Cache
{
    public class CacheManager<TMessagePayload> : ICacheManager<TMessagePayload>
        where TMessagePayload : IMessagePayload
    {
        private readonly ISerializer _serializer;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<CacheManager<TMessagePayload>> _logger;
        
        private readonly RedisValue _consumerGroupId;
        private RedisKey MainStream { get; } = new($"{typeof(TMessagePayload).Name}-main-stream");
        private RedisValue ConsumerGroupName { get; } = new($"consumer-group-{typeof(TMessagePayload).Name}");
        private IDatabase Database => _connectionMultiplexer.GetDatabase();

        public CacheManager(
            ISerializer serializer,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<CacheManager<TMessagePayload>> logger)
        {
            _serializer = serializer;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
            
            if (!Database.KeyExists(MainStream))
            {
                Database.StreamCreateConsumerGroup(MainStream, ConsumerGroupName, StreamPosition.Beginning);
                _consumerGroupId = new RedisValue(Guid.NewGuid().ToString());
            }
            else
            {
                var info = Database.StreamConsumerInfo(MainStream, ConsumerGroupName);
                _consumerGroupId = info.Any() ? info.First().Name : new RedisValue(Guid.NewGuid().ToString());
            }
        }

        public void AddMessage(TMessagePayload message)
        {
            try
            {
                Database.StreamAdd(MainStream, RedisValue.EmptyString, _serializer.Serialize(message));
            }
            catch (Exception exception)
            {
                _logger.LogError("Unable to add item to redis", exception.Message);
                throw;
            }
        }

        public async IAsyncEnumerable<TransferMessage<TMessagePayload>> ReadBatch(int batchSize)
        {
            var streamItems = await Database.StreamReadGroupAsync(
                MainStream,
                ConsumerGroupName,
                _consumerGroupId,
                StreamPosition.NewMessages,
                batchSize);
            
            foreach (var streamEntry in streamItems)
            {
                if (streamEntry.IsNull) continue;
                foreach (var entryValue in streamEntry.Values)
                {
                    var message = _serializer.Deserialize<TMessagePayload>(entryValue.Value);
                    yield return new TransferMessage<TMessagePayload>(streamEntry.Id, message);
                }
            }
        }

        public void ConfirmDelivery(string id)
        {
            try
            {
                Database.StreamAcknowledge(MainStream, ConsumerGroupName, id);
            }
            catch (Exception exception)
            {
                _logger.LogError("Unable to confirm delivery", exception.Message);
            }
        }

        public void Dispose()
        {
            _connectionMultiplexer.Dispose();
        }
    }
}