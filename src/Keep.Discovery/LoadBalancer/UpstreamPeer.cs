using Keep.Discovery.Contract;
using System;

namespace Keep.Discovery.LoadBalancer
{
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

        public int NextTries => Instance?.NextTries ?? 0;

        public TimeSpan NextTimeout
        {
            get
            {
                if (_nextTimeout == default)
                {
                    _nextTimeout = TimeSpan.FromMilliseconds(Instance?.NextTimeout ?? 0);
                }
                return _nextTimeout;
            }
        }

        public bool FreezedByFails => MaxFails != 0 && Fails >= MaxFails && DateTime.Now - Checked <= FailTimeout;
    }
}
