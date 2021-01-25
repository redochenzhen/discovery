using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery.StaticDiscovery
{
    internal class StaticDiscoveryClient : IDiscoveryClient
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<StaticDiscoveryOptions> _options;
        private readonly InstanceCache _instanceCache;
        private event EventHandler<OptionsEventArgs> OptionsChanged;

        public StaticDiscoveryClient(
            ILogger<StaticDiscoveryClient> logger,
            IOptionsMonitor<StaticDiscoveryOptions> options,
            InstanceCache instanceCache)
        {
            _logger = logger;
            _options = options;
            _instanceCache = instanceCache;
        }

        public Task DiscoverAsync()
        {
            // 对配置更改采取阈值为1秒的消抖
            Observable.FromEventPattern<OptionsEventArgs>(
                 h => OptionsChanged += h,
                 h => OptionsChanged -= h)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(x =>
                {
                    var opts = x.EventArgs.Options;
                    _instanceCache.Clear();
                    WriteToCache(opts.Mapping);
                    _logger.LogDebug("Static service mapping refreshing done.");
                });

            _options.OnChange(opts =>
            {
                OptionsChanged(this, new OptionsEventArgs
                {
                    Options = opts
                });
            });

            var options = _options.CurrentValue;
            WriteToCache(options.Mapping);
            return Task.CompletedTask;
        }

        public Task RegisterAsync()
        {
            // noop
            return Task.CompletedTask;
        }

        private void WriteToCache(IList<StaticServiceEntry> entries)
        {
            if (entries == null) return;
            foreach (var se in entries)
            {
                if (se.Instances == null) continue;
                foreach (var ie in se.Instances)
                {
                    var instance = new ServiceInstance(ie.Host, ie.Port, ie.Secure)
                    {
                        ServiceName = se.ServiceName,
                        BalancePolicy = ie.Policy,
                        Weight = ie.Weight,
                        ServiceState = ie.State,
                        ServiceType = ie.Type
                    };
                    _instanceCache.AddOrUpdate(se.ServiceName, Guid.NewGuid(), instance);
                }
            }
        }

        class OptionsEventArgs : EventArgs
        {
            public StaticDiscoveryOptions Options { get; set; }
        }
    }
}
