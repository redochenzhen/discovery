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
        private event EventHandler<OptionsEventArgs> OptionsChanged;
        InstanceCache _instanceRegistry;

        public StaticDiscoveryClient(
            ILogger<StaticDiscoveryClient> logger,
            IOptionsMonitor<StaticDiscoveryOptions> options,
            InstanceCache instanceRegistry)
        {
            _logger = logger;
            _options = options;
            _instanceRegistry = instanceRegistry;
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
                    _instanceRegistry.Clear();
                    WriteToRegistry(opts.Mapping);
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
            WriteToRegistry(options.Mapping);
            return Task.CompletedTask;
        }

        public Task RegisterAsync()
        {
            // noop
            return Task.CompletedTask;
        }

        public IList<IServiceInstance> ResolveInstances(string serviceName)
        {
            var candidates = _instanceRegistry.GetCandidates(serviceName)
                .Where(i => i.ServiceState == ServiceState.Up)
                .ToList();
            if (candidates.Count == 0)
            {
                _logger.LogWarning($"Service name '{serviceName}' can not be resolved to any instance.");
            }
            return candidates;
        }

        private void WriteToRegistry(IList<StaticServiceEntry> entries)
        {
            if (entries == null) return;
            foreach (var se in entries)
            {
                foreach (var ie in se.Instances)
                {
                    var instance = new ServiceInstance(ie.Host, ie.Port, ie.Secure)
                    {
                        ServiceName = se.ServiceName
                    };
                    _instanceRegistry.Add(se.ServiceName, Guid.NewGuid(), instance);
                }
            }
        }

        class OptionsEventArgs : EventArgs
        {
            public StaticDiscoveryOptions Options { get; set; }
        }
    }
}
