using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal class UpstreamPeer
    {
        public IServiceInstance Instance { get; set; }
        public int EffectiveWeight { get; set; }
        public int CurrentWeight { get; set; }

        public int Weight => Instance?.Weight ?? 0;
        public ServiceState State => Instance?.ServiceState ?? ServiceState.Down;
    }
}
