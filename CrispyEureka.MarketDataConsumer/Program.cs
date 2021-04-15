using System.Reflection;
using System.Threading.Tasks;
using Confluent.Kafka;
using CrispyEureka.Application.Infrastructure;
using CrispyEureka.Common;
using CrispyEureka.Domain.MarketData.Candle;
using CrispyEureka.Domain.MarketData.OrderBook;
using CrispyEureka.MarketDataConsumer.Consumers;
using CrispyEureka.MarketDataConsumer.Jobs;
using CrispyEureka.MarketDataConsumer.Profiles;
using CrispyEureka.Persistence;
using CrispyEureka.Persistence.CommandHandlers.AddCandles;
using CrispyEureka.Persistence.Profiles;
using CrispyEureka.Transfer;
using CrispyEureka.Transfer.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;

namespace CrispyEureka.MarketDataConsumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                    builder
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true)
                        .AddEnvironmentVariables())
                .ConfigureServices((hostContext, services) => services
                    .AddAutoMapper(
                        typeof(CandlesMapperProfile),
                        typeof(OrderBookMapperProfile),
                        typeof(CandlesTransferProfile),
                        typeof(OrderBookTransferProfile))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                    .AddMediatR(typeof(AddCandlesHandler).GetTypeInfo().Assembly)
                    .AddSingleton(_ => MongoUrl.Create(hostContext.Configuration.GetConnectionString("market_data_db")))
                    .AddSingleton<DbContext<Candle>>()
                    .AddSingleton<DbContext<OrderBook>>()
                    .ConfigureSettings<KafkaSettings>(hostContext.Configuration.GetSection(nameof(KafkaSettings)))
                    .ConfigureSettings<ConsumerSettings<OrderBookTransferModel>>(hostContext.Configuration.GetSection(nameof(ConsumerSettings)).GetSection(nameof(OrderBook)))
                    .ConfigureSettings<ConsumerSettings<CandleTransferModel>>(hostContext.Configuration.GetSection(nameof(ConsumerSettings)).GetSection(nameof(Candle)))
                    .AddTransient<IEurekaConsumer<OrderBookTransferModel>, EurekaConsumer<OrderBookTransferModel, OrderBook>>()
                    .AddTransient<IEurekaConsumer<CandleTransferModel>, EurekaConsumer<CandleTransferModel, Candle>>()
                    .AddTransient<IDeserializer<TransferMessage<OrderBookTransferModel>>, ProtobufDeserializer<OrderBookTransferModel>>()
                    .AddTransient<IDeserializer<TransferMessage<CandleTransferModel>>, ProtobufDeserializer<CandleTransferModel>>()
                    .AddHostedService<ConsumerJob<OrderBookTransferModel>>()
                    .AddHostedService<ConsumerJob<CandleTransferModel>>()
                )
                .UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
                    .BuildProducerLog(context.HostingEnvironment));
    }
}