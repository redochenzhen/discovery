using Keep.Discovery.Contract;
using System;

namespace Keep.Discovery.LoadBalancer
{
    internal class UpstreamPeer
    {
        private TimeSpan _failTimeout = default;

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

        public bool FreezedByFails => MaxFails != 0 && Fails >= MaxFails && DateTime.Now - Checked <= FailTimeout;
    }
}
