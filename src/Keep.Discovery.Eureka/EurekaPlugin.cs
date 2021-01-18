using Keep.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Eureka
{
    internal sealed class EurekaPlugin : PluginBase<EurekaOptions>
    {
        public EurekaPlugin() { }

        public EurekaPlugin(Action<EurekaOptions> configure) : base(configure) { }

        protected override void Configure(IServiceCollection services, IConfiguration config)
        {
            services.Configure<EurekaOptions>(config.GetSection(EurekaOptions.EUREKA_CFG_PREFIX));
            services.PostConfigure<EurekaOptions>(options =>
            {
                options.Client = new EurekaOptions.EurekaClientOptions();
                var clientSection = config.GetSection(EurekaOptions.EUREKA_CLIENT_CFG_PREFIX);
                clientSection.Bind(options.Client);
                options.Instance = new EurekaOptions.EurekaInstanceOptions();
                var instanceSection = config.GetSection(EurekaOptions.EUREKA_INSTANCE_CFG_PREFIX);
                instanceSection.Bind(options.Instance);
            });
        }

        public override void Register(IServiceCollection services)
        {
            base.Register(services);

        }
    }
}
