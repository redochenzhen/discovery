using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Keep.Discovery
{
    public interface IDiscoveryEntry
    {
        void Boot(CancellationToken stoppingToken);
    }
}
