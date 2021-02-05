using System.Text.Json.Serialization;

namespace Keep.Discovery.Contract
{
    public class InstanceEntry
    {
        public string ServiceName { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType Type { get; set; } = ServiceType.Rest;

        public string HostName { get; set; }

        public int Port { get; set; }

        public bool Secure { get; set; } = false;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceState State { get; set; } = ServiceState.Up;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BalancePolicy Balancing { get; set; } = BalancePolicy.RoundRobin;

        public int Weight { get; set; } = 1;

        public int FailTimeout { get; set; } = 1000 * 10;

        public int MaxFails { get; set; } = 1;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NextWhen NextWhen { get; set; } = NextWhen.Error | NextWhen.Timeout;

        public int NextTries { get; set; } = 0;

        public int NextTimeout { get; set; } = 0;
    }
}
