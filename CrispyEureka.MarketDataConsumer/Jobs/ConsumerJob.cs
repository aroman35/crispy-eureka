using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrispyEureka.Transfer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace CrispyEureka.MarketDataConsumer.Jobs
{
    public class ConsumerJob<TMessagePayload> : BackgroundService
        where TMessagePayload : IMessagePayload
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsumerJob<TMessagePayload>> _logger;
        private readonly ConsumerSettings<TMessagePayload> _consumerSettings;

        public ConsumerJob(
            IServiceProvider serviceProvider,
            ILogger<ConsumerJob<TMessagePayload>> logger,
            ConsumerSettings<TMessagePayload> consumerSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _consumerSettings = consumerSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_consumerSettings.IsEnabled || _consumerSettings.ConsumersCount == 0) return;
            
            _logger.LogInformation($"It will be created {_consumerSettings.ConsumersCount} consumers");
            
            await Task.WhenAll(Enumerable.Repeat(new object(), _consumerSettings.ConsumersCount)
                .Select(_ => Task.Factory.StartNew(() => Consume(stoppingToken), TaskCreationOptions.LongRunning)));
        }
        
        private async Task Consume(CancellationToken cancellationToken)
        {
            var forever = Policy.Handle<Exception>().RetryForeverAsync();
            using var scope = _serviceProvider.CreateScope();

            try
            {
                // TODO: Move consumer creation to a factory-method
                using var consumer = scope.ServiceProvider.GetRequiredService<IEurekaConsumer<TMessagePayload>>();
                await forever.ExecuteAsync(token => consumer.Consume(token), cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
                throw;
            }

        }
    }
}