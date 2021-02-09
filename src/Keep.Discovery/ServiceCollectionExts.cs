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

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExts
    {
        private static readonly Action<HttpClient> _noAction = (HttpClient _) => { };

        /// <summary>
        /// 配置服务发现使用“静态发现”
        /// </summary>
        /// <param name="options">服务发现选项</param>
        /// <returns>服务发现选项</returns>
        public static DiscoveryOptions UseStaticMapping(this DiscoveryOptions options)
        {
            options.RegisterPlugin(new StaticDiscoveryPlugin());
            return options;
        }

        /// <summary>
        /// 向IServiceCollection服务集合注册发现服务
        /// </summary>
        /// <param name="services">IServiceCollection服务集合</param>
        /// <param name="configure">配置服务发现选项的委托</param>
        /// <returns>IServiceCollection服务集合</returns>
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
            sp = services.BuildServiceProvider();
            var options = sp.GetService<IOptions<DiscoveryOptions>>().Value;
            configure(options);

            options.Plugins.ForEach(plugin => plugin.Register(services));
            services.Configure(configure);
            services.AddTransient(p =>
            {
                var handler = ActivatorUtilities.CreateInstance<DiscoveryHttpMessageHandler>(p);
                handler.DefaultTimeout = TimeSpan.FromMilliseconds(options.DefaultRequestTimeout);
                return handler;
            });
            services.AddSingleton<InstanceCache>();
            services.AddSingleton<HttpUpstreamHandler>();
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

        /// <summary>
        /// 向IServiceCollection服务集合注册带“发现”功能的HttpClient对象
        /// </summary>
        /// <param name="services">IServiceCollection服务集合</param>
        /// <param name="name">HttpClient的逻辑名称</param>
        /// <param name="configureClient">配置HttpClient的委托</param>
        /// <returns>IServiceCollection服务集合</returns>
        public static IHttpClientBuilder AddDiscoveryHttpClient(
            this IServiceCollection services,
            string name,
            Action<HttpClient> configureClient = null)
        {
            var builder = services.AddHttpClient(name, configureClient ?? _noAction)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }

        /// <summary>
        /// 向IServiceCollection服务集合注册带“发现”功能的HttpClient对象，并绑定到TClient类型
        /// </summary>
        /// <typeparam name="TClient">强类型客户端的类型，该类型会被注册为一个transient服务</typeparam>
        /// <param name="services">IServiceCollection服务集合</param>
        /// <param name="configureClient">配置HttpClient的委托</param>
        /// <returns>IHttpClientBuilder对象，用于配置client</returns>
        public static IHttpClientBuilder AddDiscoveryHttpClient<TClient>(
           this IServiceCollection services,
           Action<HttpClient> configureClient = null)
           where TClient : class
        {

            var builder = services.AddHttpClient<TClient>(configureClient ?? _noAction)
                .AddHttpMessageHandler<DiscoveryHttpMessageHandler>();
            return builder;
        }

        /// <summary>
        /// 向IServiceCollection服务集合注册带“发现”功能的HttpClient对象，并绑定到TClient类型
        /// </summary>
        /// <typeparam name="TClient">强类型客户端的类型，该类型会被注册为一个transient服务</typeparam>
        /// <typeparam name="TImpl">强类型客户端的实现类型</typeparam>
        /// <param name="services">IServiceCollection服务集合</param>
        /// <param name="configureClient">配置HttpClient的委托</param>
        /// <returns>IHttpClientBuilder对象，用于配置client</returns>
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

        /// <summary>
        /// 向IServiceCollection服务集合注册带“发现”功能的HttpClient对象，并绑定到所有被DiscoveryHttpClientAttribute标注的类型
        /// </summary>
        /// <param name="services">IServiceCollection服务集合</param>
        /// <param name="configureClient">配置HttpClient的委托</param>
        /// <param name="configureBuilder">配置IHttpClientBuilder对象的委托</param>
        /// <returns>IServiceCollection服务集合</returns>
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
