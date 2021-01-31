using System;

namespace Keep.Discovery.LoadBalancer
{
    internal interface IBalancer
    {
        UpstreamPeer Pick();
    }
}
