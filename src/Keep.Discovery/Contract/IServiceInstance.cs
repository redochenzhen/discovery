using Keep.Discovery.Contract;
using Keep.Discovery.LoadBalancer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    public interface IServiceInstance
    {
        string ServiceName { get; }

        ServiceType ServiceType { get; }

        string HostName { get; }

        int Port { get; }

        bool IsSecure { get; }

        int Weight { get; }

        ServiceState ServiceState { get; }

        BalancePolicy BalancePolicy { get; }

        Uri Uri { get; }

        IDictionary MetaData { get; }
    }
}
