using Keep.Discovery.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    public interface IServiceInstance
    {
        string ServiceName { get; }

        string HostName { get; }

        int Port { get; }

        bool IsSecure { get; }

        int Weight { get; }

        ServiceState ServiceState { get; }

        Uri Uri { get; }

        IDictionary MetaData { get; }
    }
}
