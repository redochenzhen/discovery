using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;


[assembly: InternalsVisibleTo("Keep.Discovery.Tests")]

namespace Keep.Discovery.LoadBalancer
{
    internal class RoundRobinBalancer : IBalancer
    {
        private readonly ILogger _logger;
        private IList<UpstreamPeer> _peers;
        private InstanceCacheRecord _record;
        private int _currentVer = 0;
        private int CacheVer => _record?.Version ?? 0;

        public RoundRobinBalancer(ILogger logger, InstanceCacheRecord record)
        {
            _logger = logger;
            _record = record ?? throw new ArgumentNullException(nameof(record));
            Reset();
        }

        private void Reset()
        {
            _peers = _record?.InstanceMap.Values
                .Select(ins => new UpstreamPeer
                {
                    Instance = ins,
                    EffectiveWeight = ins.Weight,
                    CurrentWeight = 0,
                })
                .ToList();
            _currentVer = CacheVer;
            if (_currentVer != 0)
            {
                _logger?.LogDebug($"Upstream peers reset due to cache vertion changes. (count: {_peers.Count}, version: {_currentVer})");
            }
        }

        public IServiceInstance Pick()
        {
            if (_currentVer != CacheVer)
            {
                Reset();
            }
            if (_peers == null || _peers.Count == 0) return null;
            if (_peers.Count == 1 && _peers[0].State == ServiceState.Up)
            {
                return _peers[0].Instance;
            }

            var best = default(UpstreamPeer);
            int total = 0;

            foreach (var peer in _peers)
            {
                if (peer.State == ServiceState.Down) continue;

                total += peer.EffectiveWeight;
                peer.CurrentWeight += peer.EffectiveWeight;

                if (best == null || best.CurrentWeight < peer.CurrentWeight)
                {
                    best = peer;
                }

                if (peer.EffectiveWeight < peer.Weight)
                {
                    peer.EffectiveWeight++;
                }
                else if (peer.EffectiveWeight > peer.Weight)
                {
                    peer.EffectiveWeight--;
                }
            }

            best.CurrentWeight -= total;
            //_logger?.LogDebug()
            return best?.Instance;
        }
    }
}
