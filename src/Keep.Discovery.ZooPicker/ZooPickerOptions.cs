using Keep.Discovery.Contract;

namespace Keep.Discovery.ZooPicker
{
    public class ZooPickerOptions
    {
        public const string CONFIG_PREFIX = "discovery:zoopicker";

        public string ConnectionString { get; set; }
        public int SessionTimeout { get; set; } = 2000 * 3;
        public int ConnectionTimeout { get; set; } = 1000 * 20;
        public string GroupName { get; set; } = "Default";

        public InstanceOptions Instance { get; set; } = new InstanceOptions();

        public class InstanceOptions
        {
            public string ServiceName { get; set; }

            public ServiceType Type { get; set; } = ServiceType.Rest;

            public int Port { get; set; } = 0;

            public bool Secure { get; set; } = false;

            public int Weight { get; set; } = 1;

            public ServiceState State { get; set; } = ServiceState.Up;

            public BalancePolicy BalancePolicy { get; set; } = BalancePolicy.RoundRobin;

            public int FailTimeout { get; set; } = 1000 * 10;

            public int MaxFails { get; set; } = 1;

            public string IpAddress { get; set; } = "127.0.0.1";

            public bool PreferIpAddress { get; set; } = true;

            public NextWhen NextWhen { get; set; } = NextWhen.Error | NextWhen.Timeout;

            public int NextTries { get; set; } = 0;

            public int NextTimeout { get; set; } = 0;
        }
    }
}
