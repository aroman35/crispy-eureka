using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Confluent.Kafka;
using CrispyEureka.Application.Commands.MarketData;
using CrispyEureka.Domain;
using CrispyEureka.Transfer;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace CrispyEureka.MarketDataConsumer.Consumers
{
    public class EurekaConsumer<TMessagePayload, TDestination> : IEurekaConsumer<TMessagePayload>
        where TMessagePayload : IMessagePayload
        where TDestination : AggregateRoot
    {
        private readonly int _batchSize;
        private readonly IConsumer<string, TransferMessage<TMessagePayload>> _consumer;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<EurekaConsumer<TMessagePayload, TDestination>> _logger;

        public EurekaConsumer(
            ConsumerSettings<TMessagePayload> consumerSettings,
            KafkaSettings kafkaSettings,
            IDeserializer<TransferMessage<TMessagePayload>> deserializer,
            IMapper mapper,
            IMediator mediator,
            ILogger<EurekaConsumer<TMessagePayload, TDestination>> logger)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _batchSize = consumerSettings.BatchSize;

            var consumerConfig = new ConsumerConfig
            {
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true,
                PartitionAssignmentStrategy = PartitionAssignmentStrategy.RoundRobin,
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = consumerSettings.ConsumerGroupId,
                ClientId = Guid.NewGuid().ToString(),
                EnableAutoCommit = true,
                AutoCommitIntervalMs = 1000,
                EnableAutoOffsetStore = true
            };

            _consumer = new ConsumerBuilder<string, TransferMessage<TMessagePayload>>(consumerConfig)
                .SetErrorHandler((_, error) => _logger.LogError(error.Reason))
                .SetStatisticsHandler((_, statistic) => _logger.LogInformation(statistic))
                .SetLogHandler((_, logMessage) =>
                    _logger.Log((LogLevel) logMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging),
                        logMessage.Message))
                .SetValueDeserializer(deserializer)
                .Build();

            _consumer.Subscribe(consumerSettings.TopicName);
        }

        public async Task Consume(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResults = ReadTopic(cancellationToken).ToList();

                    if (!consumeResults.Any()) continue;

                    foreach (var receivedMessages in consumeResults.GroupBy(x => x.Message.Key))
                    {
                        var executionResult = await Policy
                            .Handle<Exception>()
                            .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(1000))
                            .ExecuteAndCaptureAsync(
                                () => HandleResults(
                                    receivedMessages.Key,
                                    receivedMessages.Select(x => x.Message.Value.MessagePayload),
                                    cancellationToken));
                        
                        if (executionResult.FinalException != null)
                            throw executionResult.FinalException;
                    }

                    var topicPartitionOffsets = consumeResults
                        .GroupBy(x => x.Partition, result => result.TopicPartitionOffset)
                        .Select(x => x.OrderByDescending(tpo => tpo.Offset.Value).First());

                    _consumer.Commit(topicPartitionOffsets);
                }
                catch (KafkaException kafkaException)
                {
                    _logger.LogWarning(kafkaException.Error.Reason);
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception.Message);
                    throw;
                }
            }
        }

        private async Task HandleResults(string figi, IEnumerable<TMessagePayload> messages,
            CancellationToken cancellationToken)
        {
            var domainObjects = _mapper.Map<IEnumerable<TDestination>>(messages);
            var command = new AddMarketData<TDestination>(domainObjects, figi);
            await _mediator.Send(command, cancellationToken);
        }

        private IEnumerable<ConsumeResult<string, TransferMessage<TMessagePayload>>> ReadTopic(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumerResult = _consumer.Consume(TimeSpan.FromMilliseconds(30000));

                if (consumerResult == null || consumerResult.IsPartitionEOF)
                    break;

                yield return consumerResult;

                if (consumerResult.Offset % _batchSize == 0)
                    break;
            }
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}