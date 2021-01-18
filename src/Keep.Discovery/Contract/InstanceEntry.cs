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

        public bool Secure { get; set; }

        public int Weight { get; set; } = 1;
    }
}
