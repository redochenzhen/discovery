using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Keep.Discovery.Tests")]

namespace Keep.Discovery.LoadBalancer
{
    /// <summary>
    /// 带权重的随机负载均衡器
    /// </summary>
    internal sealed class RandomBalancer : BalancerBase
    {
        private readonly Random _random;

        public RandomBalancer(ILogger logger, InstanceCacheRecord record) : base(logger, record)
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
                int r = 0;
                //被设为Down后的第一次pick
                if (peer.State == ServiceState.Down && peer.Range > 0) r = -1;
                else if (peer.State == ServiceState.Up && peer.Range < 0) r = 1;

                if (r != 0)
                {
                    peer.Range *= -1;
                    ReWeight(i + 1, _peers.Count, peer.Weight * r);
                }

                if (peer.State == ServiceState.Down) continue;

                if (max < peer.Range)
                {
                    max = peer.Range;
                }
            }

            if (max == 0) return null;

            var best = default(UpstreamPeer);
            int bestIdx = 0;
            var now = DateTime.Now;
            //随机算法可能多次命中无效peer，最多尝试20次
            for (int tries = 0; tries < 20 && best == null; tries++)
            {
                int idx = _random.Next(max);
                for (int i = 0; i < _peers.Count; i++)
                {
                    peer = _peers[i];
                    if (idx < peer.Range)
                    {
                        if (TriedMark?.Get(i) ?? false) break;
                        if (peer.FreezedByFails(now)) break;
                        best = peer;
                        bestIdx = i;
                        break;
                    }
                }
            }
            //如果多次尝试后还是没有找到，则降级为简单选择一个可用peer
            if (best == null)
            {
                for (int i = 0; i < _peers.Count; i++)
                {
                    peer = _peers[i];
                    if (TriedMark?.Get(i) ?? false) continue;
                    if (peer.FreezedByFails(now)) continue;
                    best = peer;
                    bestIdx = i;
                }
            }
            if (best != null)
            {
                TriedMark?.Set(bestIdx, true);
                if (now - best.Checked > best.FailTimeout)
                {
                    best.Checked = now;
                }
            }
#if DEBUG
            var cw = _peers.Aggregate(new StringBuilder("0 - "), (a, c) => a.Append(c.Range).Append(" - "));
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
                };
                peer.Range = peer.Weight + (pre?.Range ?? 0);
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
                if (peer.Range > 0) peer.Range += w;
                else if (peer.Range < 0) peer.Range -= w;
            }
        }
    }
}
