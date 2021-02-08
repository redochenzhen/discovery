using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

[assembly: InternalsVisibleTo("Keep.Discovery.Tests")]

namespace Keep.Discovery.LoadBalancer
{
    /// <summary>
    /// 带权重的随机负载均衡器
    /// </summary>
    internal class RandomBalancer : BalancerBase
    {
        private readonly Random _random;

        public RandomBalancer(ILogger<RandomBalancer> logger, InstanceCacheRecord record) : base(logger, record)
        {
            _random = new Random();
            Reset(true);
        }

        public override UpstreamPeer Pick()
        {
            if (PeersVersion != CacheVersion)
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

            var peer = default(UpstreamPeer);
            int max = 0;
            for (int i = 0; i < _peers.Count; i++)
            {
                peer = _peers[i];
                if (peer.State == ServiceState.Down)
                {
                    //被设为Down后的第一次Pick
                    if (peer.CurrentWeight > 0)
                    {
                        peer.CurrentWeight = -peer.CurrentWeight;
                        ReWeight(i + 1, _peers.Count, -peer.EffectiveWeight);
                    }
                    continue;
                }
                else
                {
                    if (peer.CurrentWeight < 0)
                    {
                        peer.CurrentWeight = -peer.CurrentWeight;
                        ReWeight(i + 1, _peers.Count, peer.EffectiveWeight);
                    }
                }

                var deltaW = peer.Weight - peer.EffectiveWeight;
                if (deltaW != 0)
                {
                    ReWeight(i, _peers.Count, deltaW);
                }

                if (max < peer.CurrentWeight)
                {
                    max = peer.CurrentWeight;
                }
            }

            if (max == 0) return null;

            var best = default(UpstreamPeer);
            int idx = _random.Next(max);
            foreach (var p in _peers)
            {
                if (idx < p.CurrentWeight)
                {
                    best = p;
                    break;
                }
            }
#if DEBUG
            var cw = _peers.Aggregate(new StringBuilder("0 - "), (a, c) => a.Append(c.CurrentWeight).Append(" - "));
            cw.Remove(cw.Length - 3, 3);
            _logger?.LogDebug($"Current weight ranges: ({cw})");
#endif
            return best;
        }

        protected override void Reset(bool init = false)
        {
            var instances = _record.InstanceMap.Values.ToList();
            _peers = new List<UpstreamPeer>(instances.Count);
            var pre = default(UpstreamPeer);
            foreach (var ins in instances)
            {
                var peer = new UpstreamPeer
                {
                    Instance = ins,
                    EffectiveWeight = ins.Weight,
                };
                peer.CurrentWeight = peer.EffectiveWeight + (pre?.CurrentWeight ?? 0);
                _peers.Add(peer);
                pre = peer;
            }
            base.Reset(init);
        }

        private void ReWeight(int from, int to, int w)
        {
            for (int i = from; i < to; i++)
            {
                var peer = _peers[i];
                var cur = peer.CurrentWeight;
                if (cur > 0) peer.CurrentWeight += w;
                else if (cur < 0) peer.CurrentWeight -= w;
            }
        }
    }
}
