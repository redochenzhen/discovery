using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    public static class MappingExts
    {
        public static IServiceInstance ToInstance(this InstanceEntry instanceEntry)
        {
            if (instanceEntry == null)
            {
                throw new ArgumentNullException(nameof(instanceEntry));
            }
            return new ServiceInstance(
                instanceEntry.Host,
                instanceEntry.Port,
                instanceEntry.Secure)
            {
                ServiceName = instanceEntry.Name,
                ServiceState = instanceEntry.State,
                ServiceType = instanceEntry.Type,
                Weight = instanceEntry.Weight,
                BalancePolicy = instanceEntry.Policy,
                FailTimeout = instanceEntry.FailTimeout,
                MaxFails = instanceEntry.MaxFails,
            };
        }

        public static InstanceEntry ToEntry(this IServiceInstance serviceInstance)
        {
            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }
            return new InstanceEntry
            {
                Name = serviceInstance.ServiceName,
                Type = serviceInstance.ServiceType,
                Host = serviceInstance.HostName,
                Port = serviceInstance.Port,
                Secure = serviceInstance.IsSecure,
                State = serviceInstance.ServiceState,
                Weight = serviceInstance.Weight,
                Policy = serviceInstance.BalancePolicy,
                FailTimeout = serviceInstance.FailTimeout,
                MaxFails = serviceInstance.MaxFails,
            };
        }
    }
}
