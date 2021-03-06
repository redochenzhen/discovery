﻿using System;

namespace Keep.Discovery.Contract
{
    public static class MappingExts
    {
        /// <summary>
        /// InstanceEntry转ServiceInstance
        /// </summary>
        /// <param name="instanceEntry">InstanceEntry对象</param>
        /// <returns>ServiceInstance对象</returns>
        public static IServiceInstance ToInstance(this InstanceEntry instanceEntry)
        {
            if (instanceEntry == null)
            {
                throw new ArgumentNullException(nameof(instanceEntry));
            }
            return new ServiceInstance(
                instanceEntry.HostName,
                instanceEntry.Port,
                instanceEntry.Secure)
            {
                ServiceName = instanceEntry.ServiceName,
                ServiceState = instanceEntry.State,
                ServiceType = instanceEntry.Type,
                Weight = instanceEntry.Weight,
                BalancePolicy = instanceEntry.Balancing,
                FailTimeout = instanceEntry.FailTimeout,
                MaxFails = instanceEntry.MaxFails,
                NextWhen = instanceEntry.NextWhen,
                NextTries = instanceEntry.NextTries,
                NextTimeout = instanceEntry.NextTimeout,
            };
        }

        /// <summary>
        /// ServiceInstance转InstanceEntry
        /// </summary>
        /// <param name="serviceInstance">ServiceInstance对象</param>
        /// <returns>InstanceEntry对象</returns>
        internal static InstanceEntry ToEntry(this IServiceInstance serviceInstance)
        {
            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }
            return new InstanceEntry
            {
                ServiceName = serviceInstance.ServiceName,
                Type = serviceInstance.ServiceType,
                HostName = serviceInstance.HostName,
                Port = serviceInstance.Port,
                Secure = serviceInstance.IsSecure,
                State = serviceInstance.ServiceState,
                Weight = serviceInstance.Weight,
                Balancing = serviceInstance.BalancePolicy,
                FailTimeout = serviceInstance.FailTimeout,
                MaxFails = serviceInstance.MaxFails,
                NextWhen = serviceInstance.NextWhen,
                NextTries = serviceInstance.NextTries,
                NextTimeout = serviceInstance.NextTimeout,
            };
        }
    }
}
