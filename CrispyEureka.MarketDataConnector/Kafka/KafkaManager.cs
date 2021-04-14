using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using CrispyEureka.Transfer;
using Microsoft.Extensions.Logging;

namespace CrispyEureka.MarketDataConnector.Kafka
{
    public class KafkaManager : IKafkaManager
    {
        private readonly AdminClientConfig _adminClientConfig;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaManager> _logger;
        
        private static readonly SemaphoreSlim Semaphore;

        static KafkaManager()
        {
            Semaphore = new SemaphoreSlim(1, 1);
        }
        
        public KafkaManager(
            KafkaSettings settings,
            ILogger<KafkaManager> logger)
        {
            _kafkaSettings = settings;
            _logger = logger;
            _adminClientConfig = new AdminClientConfig
            {
                BootstrapServers = settings.BootstrapServers,
                SecurityProtocol = settings.SecurityProtocol
            };
        }
        
        public async Task InitTopic(string topicName)
        {
            await Semaphore.WaitAsync(TimeSpan.FromSeconds(30));
            try
            {
                var topicMetadata = GetExchangeMetadata(topicName);

                if (topicMetadata == null)
                {
                    using var adminClient = new AdminClientBuilder(_adminClientConfig).Build();
                    await adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification
                        {
                            Name = topicName,
                            NumPartitions = _kafkaSettings.PartitionsCount,
                            ReplicationFactor = _kafkaSettings.ReplicationFactor
                        }
                    }, new CreateTopicsOptions
                    {
                        RequestTimeout = TimeSpan.FromSeconds(30),
                        OperationTimeout = TimeSpan.FromSeconds(30)
                    });
                }
                else if (topicMetadata.Partitions.Count < _kafkaSettings.PartitionsCount)
                {
                    using var adminClient = new AdminClientBuilder(_adminClientConfig).Build();
                    await adminClient.CreatePartitionsAsync(new[]
                    {
                        new PartitionsSpecification
                        {
                            Topic = topicName,
                            IncreaseTo = _kafkaSettings.PartitionsCount
                        }
                    }, new CreatePartitionsOptions
                    {
                        RequestTimeout = TimeSpan.FromSeconds(30),
                        OperationTimeout = TimeSpan.FromSeconds(30)
                    });
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
            finally
            {
                Semaphore.Release();
            }
        }
        
        private TopicMetadata GetExchangeMetadata(string topicName)
        {
            using var adminClient = new AdminClientBuilder(_adminClientConfig).Build();
            return adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics.FirstOrDefault(x => x.Topic == topicName);
        }
    }
}