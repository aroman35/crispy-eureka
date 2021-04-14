using CrispyEureka.Common;
using CrispyEureka.MarketDataConnector.Cache;
using CrispyEureka.MarketDataConnector.Jobs;
using CrispyEureka.MarketDataConnector.Kafka;
using CrispyEureka.MarketDataConnector.TinkoffConnector;
using CrispyEureka.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Protobuf;

namespace CrispyEureka.MarketDataConnector
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureProducer<TMessagePayload>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TMessagePayload : IMessagePayload
        {
            services.ConfigureSettings<ProducerSettings<TMessagePayload>>(configuration.GetSection(nameof(ProducerSettings)).GetSection(typeof(TMessagePayload).Name));
            services.AddSingleton<IEurekaProducer<TMessagePayload>, EurekaProducer<TMessagePayload>>();
            services.AddTransient<ICacheManager<TMessagePayload>, CacheManager<TMessagePayload>>();
            services.AddHostedService<ProducerJob<TMessagePayload>>();

            return services;
        }

        public static IServiceCollection ConfigureApplication(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureSettings<KafkaSettings>(configuration.GetSection(nameof(KafkaSettings)));
            services.ConfigureSettings<RedisSettings>(configuration.GetSection(nameof(RedisSettings)));
            services.ConfigureSettings<TinkoffSettings>(configuration.GetSection(nameof(TinkoffSettings)));
            services.AddTransient<ISerializer, ProtobufSerializer>();
            services.AddTransient<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(provider.GetRequiredService<RedisSettings>().ConnectionString));

            return services;
        }
    }
    
}