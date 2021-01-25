using Keep.Discovery.Contract;
using Keep.Discovery.LoadBalancer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Keep.Discovery
{
    public class InstanceCache
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, InstanceCacheRecord> _cache;

        public InstanceCache(ILogger<InstanceCache> loger)
        {
            _logger = loger;
            _cache = new ConcurrentDictionary<string, InstanceCacheRecord>();
        }

        //TODO: race condition ???

        public Dictionary<string, InstanceCacheRecord> GetAll()
        {
            return _cache.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public InstanceCacheRecord GetCacheRecord(string serviceName)
        {
            if (!_cache.TryGetValue(serviceName, out var cacheRecord))
            {
                // 如果不存在，则创建一个空记录
                cacheRecord = new InstanceCacheRecord();
                _cache.TryAdd(serviceName, cacheRecord);
            }
            return cacheRecord;
        }

        public void AddOrUpdate(string serviceName, Guid serviceId, IServiceInstance instance)
        {
            if (!_cache.TryGetValue(serviceName, out var cacheRecord))
            {
                cacheRecord = new InstanceCacheRecord();
                _cache.TryAdd(serviceName, cacheRecord);
            }
            cacheRecord.InstanceMap.AddOrUpdate(serviceId, instance, (id, old) => instance);
            cacheRecord.VersionUp();
        }

        public void Remove(string serviceName, Guid serviceId)
        {
            if (!_cache.TryGetValue(serviceName, out var cacheRecord))
            {
                return;
            }
            if (cacheRecord.InstanceMap.TryRemove(serviceId, out var _))
            {
                cacheRecord.VersionUp();
            }
        }

        public IList<IServiceInstance> GetCandidates(string serviceName)
        {
            _cache.TryGetValue(serviceName, out var cacheRecord);
            return cacheRecord?.InstanceMap.Values.ToList();
        }

        public bool Exists(string serviceName, Guid serviceId)
        {
            if (!_cache.TryGetValue(serviceName, out var cacheRecord))
            {
                return false;
            }
            return cacheRecord?.InstanceMap.ContainsKey(serviceId) ?? false;
        }

        public void Clear()
        {
            foreach (var record in _cache.Values)
            {
                record.InstanceMap.Clear();
            }
        }
    }

    public class InstanceCacheRecord
    {
        public ConcurrentDictionary<Guid, IServiceInstance> InstanceMap { get; } =
            new ConcurrentDictionary<Guid, IServiceInstance>();

        public BalancePolicy Policy =>
            InstanceMap.Values.FirstOrDefault()?.BalancePolicy ?? BalancePolicy.RoundRobin;

        public int Version;

        public void VersionUp()
        {
            Interlocked.Increment(ref Version);
        }
    }
}
