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
        public int PeersVersion { get; protected set; } = 0;
        public int PeersCount => _peers?.Count ?? 0;

        /// <summary>
        /// 实例版本号（比如网络抖动导致服务实例上下线将会导致该版本号递增）
        /// </summary>
        protected int CacheVersion => _record.Version;

        protected BitArray _triedMark;

        public BalancerBase(ILogger logger, InstanceCacheRecord record)
        {
            _logger = logger;
            _record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public abstract UpstreamPeer Pick();

        protected abstract void Reset(bool init = false);

        public BitArray TriedMark { get; set; }
    }
}
