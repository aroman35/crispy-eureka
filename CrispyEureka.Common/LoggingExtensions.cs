using Microsoft.Extensions.Hosting;
using Serilog;

namespace CrispyEureka.Common
{
    public static class LoggingExtensions
    {
        public static void BuildProducerLog(
            this LoggerConfiguration loggerConfiguration,
            IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsProduction())
            {
                loggerConfiguration
                    .MinimumLevel.Warning()
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            }
            else
            {
                loggerConfiguration.BuildDevelopmentLog();
            }
        }
        
        private static void BuildDevelopmentLog(this LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console();
        }
    }
}