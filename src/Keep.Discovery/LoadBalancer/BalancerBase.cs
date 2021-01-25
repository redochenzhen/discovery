using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal abstract class BalancerBase : IBalancer
    {
        protected readonly ILogger _logger;
        protected InstanceCacheRecord _record;
        protected IList<UpstreamPeer> _peers;
        protected int _currentVer = 0;
        protected int CacheVer => _record.Version;

        public BalancerBase(ILogger logger, InstanceCacheRecord record)
        {
            _logger = logger;
            _record = record ?? throw new ArgumentNullException(nameof(record));
        }

        public abstract IServiceInstance Pick();

        protected abstract void Reset();
    }
}
