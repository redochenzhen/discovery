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
using System.Threading.Tasks;

namespace Keep.Discovery.ZooPicker
{
    internal class ZooPickerDiscoveryClient : IDiscoveryClient
    {
        private static readonly string SERVICE_ID = Guid.NewGuid().ToString();

        private readonly ILogger _logger;
        private readonly ZooPickerOptions _options;
        private readonly InstanceCache _instanceCache;

        public ZooPickerDiscoveryClient(
            ILogger<ZooPickerDiscoveryClient> logger,
            IOptions<ZooPickerOptions> options,
            InstanceCache instanceCache)
        {
            _logger = logger;
            _options = options.Value;
            _instanceCache = instanceCache;
        }

        public async Task DiscoverAsync()
        {
            var waitForExpirationTcs = new TaskCompletionSource<Void>();
            using (var zkClient = await CreateAndOpenZkClient(waitForExpirationTcs))
            {
                //var rc = Permission.Read | Permission.Create;
                var groupNode = await zkClient.ProxyNodeAsync(_options.GroupName);
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
            var waitForExpirationTcs = new TaskCompletionSource<Void>();
            using (var zkClient = await CreateAndOpenZkClient(waitForExpirationTcs))
            {
                var instanceOpts = _options.Instance;
                var serviceName = instanceOpts.ServiceName;
                var groupNode = await zkClient.ProxyNodeAsync(_options.GroupName);
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
                await instanceNode.CreateAsync(entry, Permission.All, Mode.Ephemeral);
                await waitForExpirationTcs.Task;
            }
        }

        private async Task<ZooKeeperClient> CreateAndOpenZkClient(TaskCompletionSource<Void> tcs)
        {
            var zkClient = new ZooKeeperClient(
                _options.ConnectionString,
                _options.SessionTimeout,
                _options.ConnectionTimeout);
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

        private async Task DiscoverServicesAsync(INode serviceNode, string serviceName, List<string> serviceIds)
        {
            foreach (var sid in serviceIds)
            {
                if (!Guid.TryParse(sid, out var serviceId)) continue;
                if (_instanceCache.Exists(serviceName, serviceId)) continue;
                var instanceNode = await serviceNode.ProxyJsonNodeAsync<InstanceEntry>(sid, true);
                instanceNode.NodeCreated += async (_, __) =>
                 {
                     await WriteToCacheAsync(instanceNode, serviceName, serviceId);
                     _logger.LogDebug($"[{serviceName}] add an instance: '{serviceId}'");
                 };
                instanceNode.NodeDeleted += (_, __) =>
                 {
                     _instanceCache.Remove(serviceName, serviceId);
                     _logger.LogDebug($"[{serviceName}] lost an instance: '{serviceId}'");
                 };
                instanceNode.DataChanged += (_, args) =>
                 {
                     //TODO: update
                 };
                await WriteToCacheAsync(instanceNode, serviceName, serviceId);
                _logger.LogDebug($"[{serviceName}] add an instance: '{serviceId}'");
            }
        }

        private async Task WriteToCacheAsync(IDataNode<InstanceEntry> node, string serviceName, Guid serviceId)
        {
            var entry = await node.GetDataAsync();
            var instance = new ServiceInstance(entry.Host, entry.Port, entry.Secure)
            {
                ServiceName = entry.Name,
                ServiceState = entry.State,
                ServiceType = entry.Type,
                Weight = entry.Weight,
                BalancePolicy = entry.Policy
            };
            _instanceCache.Add(serviceName, serviceId, instance);
        }
    }

    struct Void { }
}
