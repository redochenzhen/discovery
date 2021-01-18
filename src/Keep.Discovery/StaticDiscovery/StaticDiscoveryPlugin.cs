using Keep.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
