using System.Threading.Tasks;
using CrispyEureka.Common;
using CrispyEureka.Transfer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CrispyEureka.MarketDataConnector
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
                .ConfigureServices((hostContext, services) =>
                    services
                        .ConfigureApplication(hostContext.Configuration)
                        .ConfigureProducer<OrderBookTransferModel>(hostContext.Configuration)
                        .ConfigureProducer<CandleTransferModel>(hostContext.Configuration)
                )
                .UseSerilog((context, _, loggerConfiguration) => loggerConfiguration
                    .BuildProducerLog(context.HostingEnvironment));
    }
}