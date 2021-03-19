using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    /// <summary>
    /// 带权重的平滑轮训负载均衡器
    /// </summary>
    internal sealed class RoundRobinBalancer : BalancerBase
    {
        public RoundRobinBalancer(ILogger logger, InstanceCacheRecord record) : base(logger, record)
        {
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

            var best = default(UpstreamPeer);
            int bestIdx = 0;
            int total = 0;
            var now = DateTime.Now;
            for (int i = 0; i < _peers.Count; i++)
            {
                var peer = _peers[i];

                if (peer.State == ServiceState.Down) continue;

                if (TriedMark?.Get(i) ?? false) continue;

                //请求第一次失败时，可能已经有多个请求使用了相同的peer，导致多个请求失败；
                //每当FailTimeout到期时，该peer仅会放行一次，导致一个请求失败（如果该peer还没恢复的话）
                if (peer.FreezedByFails(now)) continue;

                total += peer.EffectiveWeight;
                peer.CurrentWeight += peer.EffectiveWeight;

                if (best == null || best.CurrentWeight < peer.CurrentWeight)
                {
                    best = peer;
                    bestIdx = i;
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
                TriedMark?.Set(bestIdx, true);
                best.CurrentWeight -= total;
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

        protected override void Reset(bool init = false)
        {
            _peers = _record.InstanceMap.Values
                .Select(ins => new UpstreamPeer
                {
                    Instance = ins,
                    EffectiveWeight = ins.Weight,
                })
                .ToList();
            base.Reset(init);
        }
    }
}
