using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Keep.Discovery.LoadBalancer
{
    internal abstract class BalancerBase : IBalancer
    {
        protected readonly ILogger _logger;
        protected InstanceCacheRecord _record;
        protected IList<UpstreamPeer> _peers;
        protected int _currentVersion = 0;
        protected int CacheVersion => _record.Version;

        public BalancerBase(ILogger logger, InstanceCacheRecord record)
        {
            _logger = logger;
            _record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public abstract UpstreamPeer Pick();

        protected abstract void Reset();
    }
}
