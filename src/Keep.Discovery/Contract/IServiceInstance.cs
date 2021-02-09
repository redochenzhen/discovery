using System;
using System.Collections;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 服务实例契约接口
    /// </summary>
    public interface IServiceInstance
    {
        string ServiceName { get; }

        ServiceType ServiceType { get; }

        string HostName { get; }

        int Port { get; }

        bool IsSecure { get; }

        int Weight { get; }

        int MaxFails { get; }

        int FailTimeout { get; }

        ServiceState ServiceState { get; }

        BalancePolicy BalancePolicy { get; }

        int NextTries { get; }

        int NextTimeout { get; }

        NextWhen NextWhen { get; }

        Uri Uri { get; }

        IDictionary Metadata { get; }
    }
}
