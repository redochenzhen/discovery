using System;
using System.Collections;

namespace Keep.Discovery.LoadBalancer
{
    internal interface IBalancer
    {
        int PeersVersion { get; }

        int PeersCount { get; }

        BitArray TriedMark { get; set; }

        UpstreamPeer Pick();
    }
}
