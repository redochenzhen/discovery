using Keep.Discovery;
using Keep.Discovery.ZooPicker;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DiscoveryOptionsExts
    {
        public static DiscoveryOptions UseZooPicker(this DiscoveryOptions options, Action<ZooPickerOptions> configure)
        {
            options.RegisterPlugin(new ZooPickerPlugin(configure));
            return options;
        }

        public static DiscoveryOptions UseZooPicker(this DiscoveryOptions options)
        {
            options.RegisterPlugin(new ZooPickerPlugin());
            return options;
        }
    }
}
