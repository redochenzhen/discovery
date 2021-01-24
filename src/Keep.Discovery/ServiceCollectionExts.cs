using Keep.Discovery;
using Keep.Discovery.Http;
using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Keep.Discovery.Pump;
using Keep.Discovery.StaticDiscovery;
using Keep.Discovery.Worker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExts
    {
        private static readonly Action<HttpClient> _noAction = (HttpClient _) => { };

        public static DiscoveryOptions UseStaticMapping(this DiscoveryOptions options)
        {
            options.RegisterPlugin(new StaticDiscoveryPlugin());
            return options;
        }

        public static IServiceCollection AddDiscovery(this IServiceCollection services, Action<DiscoveryOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.AddTransient<IStartupFilter, StartupFilter>();

            var sp = services.BuildServiceProvider();
            var config = sp.GetRequiredService<IConfiguration>();
            services.Configure<DiscoveryOptions>(config.GetSection(DiscoveryOptions.CONFIG_PREFIX));
            var options = sp.GetService<IOptions<DiscoveryOptions>>().Value;
            configure(options);


            options.Plugins.ForEach(plugin => plugin.Register(services));
            services.Configure(configure);
            services.AddTransient<DiscoveryHttpMessageHandler>();
            services.AddSingleton<InstanceCache>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<IBalancerFactory, BalancerFactory>();

            services.TryAddEnumerable(
            new[]
            {
                ServiceDescriptor.Singleton<IWorker, RegistrationWorker>(),
                ServiceDescriptor.Singleton<IWorker, DiscoveryWorker>(),
                //ServiceDescriptor.Singleton<IDiscoveryWorker, HeartbeatWorker>()
            });

            services.AddSingleton<IDiscoveryEntry, DiscoveryEntry>();
            services.AddHostedService<DiscoveryEntry>();
            return services;
        }

        public static IHttpClientBuilder AddDiscoveryHttpClient<TClient>(
           this IServiceCollection services,
           Action<HttpClient> configureClient = null)
           where TClient : class
        {

            var builder = services.AddHttpClient<TClient>(configureClient ?? _noAction)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }

        public static IHttpClientBuilder AddDiscoveryHttpClient<TClient, TImpl>(
            this IServiceCollection services,
            Action<HttpClient> configureClient = null)
            where TClient : class
            where TImpl : class, TClient
        {

            var builder = services.AddHttpClient<TClient, TImpl>(configureClient ?? _noAction)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }

        public static IServiceCollection AddDiscoveryHttpClient(
            this IServiceCollection services,
            Action<HttpClient> configureClient = null,
            Action<IHttpClientBuilder> configureBuilder = null)
        {
            var ts = QueryType();
            foreach (var t in ts)
            {
                var name = t.Item1.Name;
                var builder = services.AddHttpClient(name, configureClient ?? _noAction)
                    .AddHttpMessageHandler<DiscoveryHttpMessageHandler>(); ;
                configureBuilder?.Invoke(builder);
                services.AddTransient(t.Item1, sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient(name);
                    return ActivatorUtilities.CreateInstance(sp, t.Item2, httpClient);
                });
            }
            return services;
        }

        private static IEnumerable<(Type, Type)> QueryType()
        {
            var x = Assembly.GetEntryAssembly()
                .DefinedTypes
                .SelectMany(ti =>
                {
                    var t = ti.AsType();
                    var dhcAttr = t.GetCustomAttribute<DiscoveryHttpClientAttribute>();
                    if (dhcAttr == null) return Enumerable.Empty<(Type, Type)>();
                    var itArr = t.GetInterfaces();
                    if (itArr.Length == 0)
                    {
                        return Enumerable.Repeat((t, t), 1);
                    }
                    else if (itArr.Length == 1)
                    {
                        return Enumerable.Repeat((itArr[0], t), 1);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                });
            return x;
        }
    }
}
