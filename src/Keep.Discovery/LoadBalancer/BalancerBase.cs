using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Keep.Discovery.LoadBalancer
{
    internal abstract class BalancerBase : IBalancer
    {
        protected readonly ILogger _logger;
        protected InstanceCacheRecord _record;
        protected IList<UpstreamPeer> _peers;
        protected BitArray _triedMark;
        /// <summary>
        /// 实例缓存版本号（比如网络抖动引起服务实例上下线，将会导致该版本号递增）
        /// </summary>
        protected int CacheVersion => _record.Version;

        public BitArray TriedMark { get; set; }
        public int PeersVersion { get; protected set; } = 0;
        public int PeersCount => _peers?.Count ?? 0;

        public BalancerBase(ILogger logger, InstanceCacheRecord record)
        {
            _logger = logger;
            _record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public abstract UpstreamPeer Pick();

        protected virtual void Reset(bool init = false)
        {
            if (!init)
            {
                //上游服务端版本号变动，可能引起“已尝试”标记失效
                TriedMark = new BitArray(TriedMark.Length);
            }
            PeersVersion = CacheVersion;
#if DEBUG
            if (PeersVersion != 0)
            {
                _logger?.LogDebug($"Upstream peers reset due to cache vertion changing. (count: {_peers.Count}, version: {PeersVersion})");
            }
#endif
        }
    }
}
