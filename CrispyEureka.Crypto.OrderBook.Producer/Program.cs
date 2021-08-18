using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CrispyEureka.Crypto.OrderBook.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                    configuration
                        .Enrich.FromLogContext()
                        .WriteTo.Async(x => x.Console(LogEventLevel.Information))
                )
                .ConfigureServices((hostContext, services) => { services.AddHostedService<Worker>(); });
    }
}