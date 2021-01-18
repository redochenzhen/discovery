using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keep.Discovery
{
    public class InstanceCache
    {
        private readonly ILogger _logger;
        // ServiceName -> InstanceMap
        // InstanceMap :: ServiceId -> Instance
        private readonly Dictionary<string, ConcurrentDictionary<Guid, IServiceInstance>> _map;

        public InstanceCache(ILogger<InstanceCache> loger)
        {
            _logger = loger;
            _map = new Dictionary<string, ConcurrentDictionary<Guid, IServiceInstance>>();
        }

        //TODO: race condition ???

        public Dictionary<string, ConcurrentDictionary<Guid, IServiceInstance>> GetAll()
        {
            return _map;
        }

        public void Add(string serviceName, Guid serviceId, IServiceInstance instance)
        {
            if (!_map.TryGetValue(serviceName, out var instanceMap))
            {
                instanceMap = new ConcurrentDictionary<Guid, IServiceInstance>();
                _map.Add(serviceName, instanceMap);
            }
            instanceMap.AddOrUpdate(serviceId, instance, (id, old) => instance);
        }

        public void Remove(string serviceName, Guid serviceId)
        {
            if (!_map.TryGetValue(serviceName, out var instanceMap))
            {
                return;
            }
            instanceMap.TryRemove(serviceId, out var _);
        }

        public IList<IServiceInstance> GetCandidates(string serviceName)
        {
            _map.TryGetValue(serviceName, out var instanceMap);
            return instanceMap?.Values.ToList();
        }

        public bool Exists(string serviceName, Guid serviceId)
        {
            if (!_map.TryGetValue(serviceName, out var instanceMap))
            {
                return false;
            }
            return instanceMap?.ContainsKey(serviceId) ?? false;
        }

        public void Clear()
        {
            _map.Clear();
        }
    }
}
