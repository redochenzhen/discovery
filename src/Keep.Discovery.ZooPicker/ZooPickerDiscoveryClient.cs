using Keep.Common.Utilitiy;
using Keep.Discovery.Contract;
using Keep.Discovery.Exceptions;
using Keep.ZooProxy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static Keep.Discovery.ZooPicker.ZooPickerOptions;

namespace Keep.Discovery.ZooPicker
{
    internal class ZooPickerDiscoveryClient : IDiscoveryClient
    {
        private static readonly string SERVICE_ID = Guid.NewGuid().ToString();

        private readonly ILogger _logger;
        private readonly IOptionsMonitor<ZooPickerOptions> _options;
        private readonly InstanceCache _instanceCache;
        private event EventHandler<OptionsEventArgs> OptionsChanged;

        public ZooPickerDiscoveryClient(
            ILogger<ZooPickerDiscoveryClient> logger,
            IOptionsMonitor<ZooPickerOptions> options,
            InstanceCache instanceCache)
        {
            _logger = logger;
            _options = options;
            _instanceCache = instanceCache;
        }

        public async Task DiscoverAsync()
        {
            var options = _options.CurrentValue;
            var waitForExpirationTcs = new TaskCompletionSource<Void>();
            using (var zkClient = await CreateAndOpenZkClient(waitForExpirationTcs))
            {
                //var rc = Permission.Read | Permission.Create;
                var groupNode = await zkClient.ProxyNodeAsync(options.GroupName);
                await groupNode.CreateAsync(Permission.All, Mode.Persistent, true);
                var serviceNames = await groupNode.GetChildrenAsync();
                foreach (var sn in serviceNames)
                {
                    var serviceNode = await groupNode.ProxyNodeAsync(sn, true);
                    await serviceNode.CreateAsync(Permission.All, Mode.Persistent, true);
                    serviceNode.ChildrenChanged += async (_, args) =>
                      {
                          if (args.Children == null || args.Children.Count == 0) return;
                          await DiscoverServicesAsync(serviceNode, sn, args.Children);
                      };
                    var serviceIds = await serviceNode.GetChildrenAsync();
                    await DiscoverServicesAsync(serviceNode, sn, serviceIds);
                }
                await waitForExpirationTcs.Task;
            }
        }

        public async Task RegisterAsync()
        {
            var options = _options.CurrentValue;
            var waitForExpirationTcs = new TaskCompletionSource<Void>();
            var dispos = _options.OnChange(opts =>
            {
                OptionsChanged(this, new OptionsEventArgs
                {
                    Options = opts
                });
            });
            using (dispos)
            using (var zkClient = await CreateAndOpenZkClient(waitForExpirationTcs))
            {
                var instanceOpts = options.Instance;
                var serviceName = instanceOpts.ServiceName;
                var groupNode = await zkClient.ProxyNodeAsync(options.GroupName);
                await groupNode.CreateAsync(Permission.All, Mode.Persistent, true);
                var serviceNode = await groupNode.ProxyNodeAsync(serviceName);
                await serviceNode.CreateAsync(Permission.All, Mode.Persistent, true);
                var instanceNode = await serviceNode.ProxyJsonNodeAsync<InstanceEntry>(SERVICE_ID);
                var hostName = DnsHelper.ResolveHostName();
                if (instanceOpts.PreferIpAddress)
                {
                    hostName = instanceOpts.IpAddress ?? DnsHelper.ResolveIpAddress(hostName);
                }
                var entry = new InstanceEntry
                {
                    Name = serviceName,
                    Host = hostName,
                    Port = instanceOpts.Port,
                    Type = instanceOpts.ServiceType,
                    State = instanceOpts.ServiceState,
                    Secure = instanceOpts.IsSecure,
                    Weight = instanceOpts.Weight,
                    Policy = instanceOpts.BalancePolicy
                };
                Observable.FromEventPattern<OptionsEventArgs>(
                    h => OptionsChanged += h,
                    h => OptionsChanged -= h)
                   .Throttle(TimeSpan.FromSeconds(1))
                   .Subscribe(async x =>
                   {
                       var opts = x.EventArgs.Options;
                       var insOpts = opts.Instance;
                       if (ShouldUpdate(entry, insOpts))
                       {
                           entry.State = insOpts.ServiceState;
                           entry.Weight = insOpts.Weight;
                           await instanceNode.SetDataAsync(entry);
                           _logger.LogDebug($"[{serviceName}] state: {entry.State}, weight: {entry.Weight}");
                       }
                   });

                await instanceNode.CreateAsync(entry, Permission.All, Mode.Ephemeral);
                await waitForExpirationTcs.Task;
            }
        }

        private bool ShouldUpdate(InstanceEntry entry, ZooKeeperInstanceOptions insOpts)
        {
            return entry.State != insOpts.ServiceState ||
                entry.Weight != insOpts.Weight;
        }

        private async Task<ZooKeeperClient> CreateAndOpenZkClient(TaskCompletionSource<Void> tcs)
        {
            var options = _options.CurrentValue;
            var zkClient = new ZooKeeperClient(
                options.ConnectionString,
                options.SessionTimeout,
                options.ConnectionTimeout);
            await zkClient.OpenAsync();
            zkClient.FirstConnected += (_, __) =>
            {
                _logger.LogInformation("Connect to zookeeper server successfully.");
            };
            zkClient.ReConnected += (_, __) =>
            {
                _logger.LogInformation("Reconnect to zookeeper server successfully.");
            };
            zkClient.Disconnected += (_, __) =>
            {
                _logger.LogInformation("Lost connection to zookeeper server.");
            };
            zkClient.SessionExpired += (_, __) =>
            {
                tcs.SetException(new UnhealthyException(
                    "Session of this zookeeper connection is expired.",
                    new KeeperException.SessionExpiredException()));
            };
            return zkClient;
        }

        private async Task DiscoverServicesAsync(INodeProxy serviceNode, string serviceName, List<string> serviceIds)
        {
            foreach (var sid in serviceIds)
            {
                if (!Guid.TryParse(sid, out var serviceId)) continue;
                if (_instanceCache.Exists(serviceName, serviceId)) continue;
                var instanceNode = await serviceNode.ProxyJsonNodeAsync<InstanceEntry>(sid, true);
                instanceNode.NodeCreated += async (_, __) =>
                 {
                     var ent = await instanceNode.GetDataAsync();
                     WriteToCache(ent, serviceName, serviceId);
                     _logger.LogDebug($"[{serviceName}] add an instance: '{serviceId}'");
                 };
                instanceNode.NodeDeleted += (_, __) =>
                 {
                     _instanceCache.Remove(serviceName, serviceId);
                     _logger.LogDebug($"[{serviceName}] lost an instance: '{serviceId}'");
                 };
                instanceNode.DataChanged += (_, args) =>
                 {
                     WriteToCache(args.Data, serviceName, serviceId);
                     _logger.LogDebug($"[{serviceName}] update an instance: '{serviceId}'");
                 };
                var entry = await instanceNode.GetDataAsync();
                WriteToCache(entry, serviceName, serviceId);
                _logger.LogDebug($"[{serviceName}] add an instance: '{serviceId}'");
            }
        }

        private void WriteToCache(InstanceEntry entry, string serviceName, Guid serviceId)
        {
            var instance = new ServiceInstance(entry.Host, entry.Port, entry.Secure)
            {
                ServiceName = entry.Name,
                ServiceState = entry.State,
                ServiceType = entry.Type,
                Weight = entry.Weight,
                BalancePolicy = entry.Policy
            };
            _instanceCache.AddOrUpdate(serviceName, serviceId, instance);
        }

        class OptionsEventArgs : EventArgs
        {
            public ZooPickerOptions Options { get; set; }
        }
    }

    struct Void { }
}
