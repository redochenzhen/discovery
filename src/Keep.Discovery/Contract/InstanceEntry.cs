using Keep.Discovery.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Keep.Discovery.Contract
{
    public class InstanceEntry
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType Type { get; set; } = ServiceType.Rest;

        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceState State { get; set; } = ServiceState.Up;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BalancePolicy Policy { get; set; } = BalancePolicy.RoundRobin;

        public bool Secure { get; set; }

        public int Weight { get; set; } = 1;

        public int FailTimeout { get; set; } = 1000 * 10;

        public int MaxFails { get; set; } = 1;
    }
}
