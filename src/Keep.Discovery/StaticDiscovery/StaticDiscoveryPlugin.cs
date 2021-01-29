using Keep.Common.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keep.Discovery.StaticDiscovery
{
    internal class StaticDiscoveryPlugin : PluginBase<StaticDiscoveryOptions>
    {
        protected override void Configure(IServiceCollection services, IConfiguration config)
        {
            services.Configure<StaticDiscoveryOptions>(config.GetSection(StaticDiscoveryOptions.CONFIG_PREFIX));
        }

        public override void Register(IServiceCollection services)
        {
            base.Register(services);
            services.AddSingleton<IDiscoveryClient, StaticDiscoveryClient>();
        }
    }
}
