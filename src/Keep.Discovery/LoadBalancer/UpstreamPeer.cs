using Keep.Discovery.Contract;
using System;
using System.Threading;

namespace Keep.Discovery.LoadBalancer
{
    /// <summary>
    /// 上游服务端：包含服务实例信息，负载均衡、错误恢复、重试等行为的参数
    /// </summary>
    internal class UpstreamPeer
    {
        private TimeSpan _failTimeout = default;
        private TimeSpan _nextTimeout = default;

        public IServiceInstance Instance { get; set; }

        public int EffectiveWeight { get; set; }

        public int CurrentWeight { get; set; }

        public int Weight => Instance?.Weight ?? 0;

        public ServiceState State => Instance?.ServiceState ?? ServiceState.Down;

        public TimeSpan FailTimeout
        {
            get
            {
                if (_failTimeout == default)
                {
                    _failTimeout = TimeSpan.FromMilliseconds(Instance?.FailTimeout ?? 0);
                }
                return _failTimeout;
            }
        }

        public DateTime Checked { get; set; }

        public int MaxFails => Instance?.MaxFails ?? 0;

        public int Fails { get; set; }

        public int Connections { get; set; }

        public NextWhen NextWhen => Instance?.NextWhen ?? NextWhen.None;

        /// <summary>
        /// 尝试“下一个”的次数，0表示一直尝试
        /// </summary>
        public int NextTries => Instance?.NextTries ?? 0;

        /// <summary>
        /// 尝试“下一个”的超时时间
        /// </summary>
        public TimeSpan NextTimeout
        {
            get
            {
                if (_nextTimeout == default && Instance != null)
                {
                    _nextTimeout = Instance.NextTimeout == 0 ?
                        Timeout.InfiniteTimeSpan :
                        TimeSpan.FromMilliseconds(Instance.NextTimeout);
                }
                return _nextTimeout;
            }
        }

        /// <summary>
        /// 由于请求失败而临时冻结
        /// </summary>
        public bool FreezedByFails => MaxFails != 0 && Fails >= MaxFails && DateTime.Now - Checked <= FailTimeout;
    }
}
