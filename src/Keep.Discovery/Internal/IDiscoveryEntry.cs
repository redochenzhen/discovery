using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Keep.Discovery.Internal
{
    internal interface IDiscoveryEntry
    {
        void Boot(CancellationToken stoppingToken);
    }
}
