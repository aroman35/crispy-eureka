using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using CrispyEureka.Transfer;
using Microsoft.Extensions.Logging;
using Polly;

namespace CrispyEureka.MarketDataConnector.Kafka
{
    public class EurekaProducer<TMessagePayload> : IEurekaProducer<TMessagePayload>
        where TMessagePayload : IMessagePayload
    {
        private readonly IProducer<string, TransferMessage<TMessagePayload>> _producer;
        private readonly ICacheManager<TMessagePayload> _cacheManager;
        private readonly ProducerSettings _producerSettings;
        private readonly IKafkaManager _kafkaManager;
        private readonly ILogger<EurekaProducer<TMessagePayload>> _logger;
        
        private int _currentBatchSize;

        public EurekaProducer(
            ICacheManager<TMessagePayload> cacheManager,
            ProducerSettings<TMessagePayload> producerSettings,
            KafkaSettings kafkaSettings,
            ILogger<EurekaProducer<TMessagePayload>> logger,
            IKafkaManager kafkaManager)
        {
            _cacheManager = cacheManager;
            _producerSettings = producerSettings;
            _logger = logger;
            _kafkaManager = kafkaManager;

            var producerConfig = new ProducerConfig
            {
                ClientId = producerSettings.ProducerId,
                Partitioner = Partitioner.Murmur2Random,
                BootstrapServers = kafkaSettings.BootstrapServers,
                SecurityProtocol = kafkaSettings.SecurityProtocol
            };

            _producer = new ProducerBuilder<string, TransferMessage<TMessagePayload>>(producerConfig)
                .SetValueSerializer(new ProtobufSerializer<TMessagePayload>())
                .SetErrorHandler((_, error) => _logger.LogError(error.Reason))
                .SetLogHandler((_, logMessage) => _logger.Log((LogLevel)logMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging), logMessage.Message))
                .Build();
        }

        public async Task InitializeExchange()
        {
            await _kafkaManager.InitTopic(_producerSettings.TopicName);
        }
        
        public async Task StartProducingJob(CancellationToken cancellationToken)
        {
            await Policy.Handle<Exception>()
                .WaitAndRetryForeverAsync(
                    _ => TimeSpan.FromSeconds(1),
                    (exception, _) => _logger.LogError("Failed to produce. Job will retry in 1 sec", exception.Message))
                .ExecuteAsync(() => _cacheManager
                    .ReadBatch(_producerSettings.BatchSize)
                    .ForEachAsync(Send, cancellationToken));
        }
        
        private void Send(TransferMessage<TMessagePayload> transferMessage)
        {
            _producer.Produce(
                _producerSettings.TopicName,
                new Message<string, TransferMessage<TMessagePayload>>
                {
                    Key = transferMessage.Figi,
                    Value = transferMessage
                },
                DeliveryHandler);
            

            Interlocked.Increment(ref _currentBatchSize);

            if (Interlocked.CompareExchange(ref _currentBatchSize, 0, _producerSettings.BatchSize) != _producerSettings.BatchSize) return;
            _logger.LogInformation($"Inserted {_producerSettings.BatchSize}");
            _producer.Flush(TimeSpan.FromMilliseconds(_producerSettings.SendTimeoutMs));
        }
        
        private void DeliveryHandler(DeliveryReport<string, TransferMessage<TMessagePayload>> deliveryReport)
        {
            if (!deliveryReport.Error.IsError)
            {
                try
                {
                    _cacheManager.ConfirmDelivery(deliveryReport.Value.Key);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning("Message was not removed from a cache storage, it will perhaps be sent again",
                        deliveryReport.Value.Figi,
                        exception.Message);
                }
            }
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}