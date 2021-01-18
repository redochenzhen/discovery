using Keep.Discovery;
using Keep.Discovery.Eureka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiscoveryOptionsExts
    {
        public static DiscoveryOptions UseEureka(this DiscoveryOptions options, Action<EurekaOptions> configure)
        {
            options.RegisterPlugin(new EurekaPlugin(configure));
            return options;
        }

        public static DiscoveryOptions UseEureka(this DiscoveryOptions options)
        {
            options.RegisterPlugin(new EurekaPlugin());
            return options;
        }
    }
}
