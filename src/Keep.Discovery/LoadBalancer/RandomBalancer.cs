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
    internal class RandomBalancer : BalancerBase
    {
        private readonly Random _random;

        public RandomBalancer(ILogger<RandomBalancer> logger, InstanceCacheRecord record) : base(logger, record)
        {
            _random = new Random();
            Reset();
        }

        public override IServiceInstance Pick()
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

                if (peer.EffectiveWeight < peer.Weight)
                {
                    ReWeight(i, _peers.Count, 1);
                }
                else if (peer.EffectiveWeight < peer.Weight)
                {
                    ReWeight(i, _peers.Count, -1);
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
            return best?.Instance;
        }

        protected override void Reset()
        {
            var instances = _record.InstanceMap.Values.ToList();
            int count = instances.Count;
            _peers = new List<UpstreamPeer>(count);
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
            _currentVer = CacheVer;
            if (_currentVer != 0)
            {
                _logger?.LogDebug($"Upstream peers reset due to cache vertion changing. (count: {_peers.Count}, version: {_currentVer})");
            }
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
