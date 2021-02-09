using Keep.Discovery.Contract;
using System;

namespace Keep.Discovery.ZooPicker
{
    public static class MappingExts
    {
        /// <summary>
        /// ZooPickerOptions.InstanceOptions转InstanceEntry
        /// </summary>
        /// <param name="instanceOptions">ZooPickerOptions.InstanceOptions对象</param>
        /// <returns>InstanceEntry对象</returns>
        public static InstanceEntry ToEntry(this ZooPickerOptions.InstanceOptions instanceOptions)
        {
            if (instanceOptions == null)
            {
                throw new ArgumentNullException(nameof(instanceOptions));
            }
            int port = instanceOptions.Port;
            if (port == 0)
            {
                port = instanceOptions.Secure ? 443 : 80;
            }
            return new InstanceEntry
            {
                ServiceName = instanceOptions.ServiceName,
                Port = port,
                Type = instanceOptions.Type,
                State = instanceOptions.State,
                Secure = instanceOptions.Secure,
                Weight = instanceOptions.Weight,
                Balancing = instanceOptions.BalancePolicy,
                FailTimeout = instanceOptions.FailTimeout,
                MaxFails = instanceOptions.MaxFails,
                NextWhen = instanceOptions.NextWhen,
                NextTries = instanceOptions.NextTries,
                NextTimeout = instanceOptions.NextTimeout,
            };
        }

    }
}
