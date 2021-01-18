using Keep.Discovery.Http;
using Refit;
using System;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExts
    {
        public static IHttpClientBuilder AddDiscoveryRefitClient<TClient>(
           this IServiceCollection services,
           RefitSettings settings = null)
           where TClient : class
        {

            var builder = services.AddRefitClient<TClient>(settings)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }

        public static IHttpClientBuilder AddDiscoveryRefitClient(
           this IServiceCollection services,
           Type refitInterfaceType,
           RefitSettings settings = null)
        {
            var builder = services.AddRefitClient(refitInterfaceType, settings)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }
    }
}

