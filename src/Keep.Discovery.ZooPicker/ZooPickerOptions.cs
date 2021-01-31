﻿using System;
using System.Collections.Generic;
using System.Text;
using Keep.Common;
using Keep.Discovery.Contract;
using Keep.Discovery.LoadBalancer;

namespace Keep.Discovery.ZooPicker
{
    public class ZooPickerOptions
    {
        public const string CONFIG_PREFIX = "discovery:zoopicker";

        public string ConnectionString { get; set; }
        public int SessionTimeout { get; set; } = 2000 * 3;
        public int ConnectionTimeout { get; set; } = 1000 * 20;
        public string GroupName { get; set; } = "Default";

        public InstanceOptions Instance { get; set; }

        public class InstanceOptions
        {
            public string ServiceName { get; set; }
            public int Port { get; set; } = 80;
            public bool IsSecure { get; set; } = false;
            public int Weight { get; set; } = 1;
            public ServiceState ServiceState { get; set; } = ServiceState.Up;
            public ServiceType ServiceType { get; set; } = ServiceType.Rest;
            public BalancePolicy BalancePolicy { get; set; } = BalancePolicy.RoundRobin;
            public int FailTimeout { get; set; } = 1000 * 10;
            public int MaxFails { get; set; } = 1;
            public string IpAddress { get; set; }
            public bool PreferIpAddress { get; set; }
        }
    }
}
