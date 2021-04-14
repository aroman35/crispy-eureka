using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrispyEureka.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureSettings<TSettings>(this IServiceCollection services, IConfigurationSection section)
            where TSettings : class, new()
        {
            services
                .Configure<TSettings>(section)
                .AddSingleton(provider => provider.GetRequiredService<IOptions<TSettings>>().Value);

            return services;
        }
    }
}