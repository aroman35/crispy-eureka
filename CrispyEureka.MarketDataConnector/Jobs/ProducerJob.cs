using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrispyEureka.Transfer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CrispyEureka.MarketDataConnector.Jobs
{
    public class ProducerJob<TMessagePayload> : BackgroundService
        where TMessagePayload : IMessagePayload
    {
        private readonly IEurekaProducer<TMessagePayload> _producer;
        private readonly ILogger<ProducerJob<TMessagePayload>> _logger;

        public ProducerJob(
            IEurekaProducer<TMessagePayload> producer,
            ILogger<ProducerJob<TMessagePayload>> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _producer.InitializeExchange();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();
                await _producer.StartProducingJob(stoppingToken);
                stopwatch.Stop();
                _logger.LogTrace($"Operation took {stopwatch.Elapsed:g}");
            }
        }
    }
}