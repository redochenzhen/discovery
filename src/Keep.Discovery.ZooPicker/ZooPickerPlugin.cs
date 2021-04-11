using Keep.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Keep.Discovery.ZooPicker
{
    internal sealed class ZooPickerPlugin : PluginBase<ZooPickerOptions>
    {
        public ZooPickerPlugin() { }

        public ZooPickerPlugin(Action<ZooPickerOptions> configure) : base(configure) { }

        protected override void Configure(IServiceCollection services, IConfiguration config)
        {
            services.Configure<ZooPickerOptions>(config.GetSection(ZooPickerOptions.CONFIG_PREFIX));
        }

        public override void Register(IServiceCollection services)
        {
            base.Register(services);
            services.AddSingleton<IDiscoveryClient, ZooPickerDiscoveryClient>();
        }
    }
}
