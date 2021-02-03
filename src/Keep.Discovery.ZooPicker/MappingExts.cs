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
            return new InstanceEntry
            {
                Name = instanceOpts.ServiceName,
                Port = instanceOpts.Port,
                Type = instanceOpts.ServiceType,
                State = instanceOpts.ServiceState,
                Secure = instanceOpts.IsSecure,
                Weight = instanceOpts.Weight,
                Policy = instanceOpts.BalancePolicy,
                FailTimeout = instanceOpts.FailTimeout,
                MaxFails = instanceOpts.MaxFails,
                NextWhen = instanceOpts.Next.When,
                NextTries = instanceOpts.Next.Tries,
                NextTimeout = instanceOpts.Next.Timeout,
            };
        }

    }
}
