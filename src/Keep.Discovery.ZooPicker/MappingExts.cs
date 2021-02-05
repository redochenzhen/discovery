using Keep.Discovery.Contract;
using System;

namespace Keep.Discovery.ZooPicker
{
    public static class MappingExts
    {
        public static InstanceEntry ToEntry(this ZooPickerOptions.InstanceOptions instanceOpts)
        {
            if (instanceOpts == null)
            {
                throw new ArgumentNullException(nameof(instanceOpts));
            }
            int port = instanceOpts.Port;
            if (port == 0)
            {
                port = instanceOpts.Secure ? 443 : 80;
            }
            return new InstanceEntry
            {
                ServiceName = instanceOpts.ServiceName,
                Port = port,
                Type = instanceOpts.Type,
                State = instanceOpts.State,
                Secure = instanceOpts.Secure,
                Weight = instanceOpts.Weight,
                Balancing = instanceOpts.BalancePolicy,
                FailTimeout = instanceOpts.FailTimeout,
                MaxFails = instanceOpts.MaxFails,
                NextWhen = instanceOpts.NextWhen,
                NextTries = instanceOpts.NextTries,
                NextTimeout = instanceOpts.NextTimeout,
            };
        }

    }
}
