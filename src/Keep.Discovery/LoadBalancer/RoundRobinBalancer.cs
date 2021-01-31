using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal class RoundRobinBalancer : BalancerBase
    {
        public RoundRobinBalancer(ILogger logger, InstanceCacheRecord record) : base(logger, record)
        {
            Reset();
        }

        public override UpstreamPeer Pick()
        {
            if (_currentVersion != CacheVersion)
            {
                Reset();
            }
            if (_peers == null || _peers.Count == 0)
            {
                return null;
            }
            if (_peers.Count == 1 && _peers[0].State == ServiceState.Up)
            {
                return _peers[0];
            }

            var best = default(UpstreamPeer);
            int total = 0;

            foreach (var peer in _peers)
            {
                if (peer.State == ServiceState.Down) continue;

                if (peer.FreezedByFails) continue;

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
            if (best != null)
            {
                best.CurrentWeight -= total;
                var now = DateTime.Now;
                if (now - best.Checked > best.FailTimeout)
                {
                    best.Checked = now;
                }
            }
#if DEBUG
            var cw = _peers.Aggregate(new StringBuilder(), (a, c) => a.Append(c.CurrentWeight).Append(", "));
            cw.Remove(cw.Length - 2, 2);
            _logger?.LogDebug($"Current weights: ({cw})");
#endif
            return best;
        }

        protected override void Reset()
        {
            _peers = _record.InstanceMap.Values
                .Select(ins => new UpstreamPeer
                {
                    Instance = ins,
                    EffectiveWeight = ins.Weight,
                    CurrentWeight = 0,
                })
                .ToList();
            _currentVersion = CacheVersion;
            if (_currentVersion != 0)
            {
                _logger?.LogDebug($"Upstream peers reset due to cache vertion changing. (count: {_peers.Count}, version: {_currentVersion})");
            }
        }
    }
}
