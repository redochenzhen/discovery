using Keep.Common.Framework;
using System;

namespace Keep.Discovery
{
    public class DiscoveryOptions : OptionsBase
    {
        public const string CONFIG_PREFIX = "discovery";

        public bool ShouldDiscover { get; set; } = false;

        public bool ShouldRegister { get; set; } = false;

        public int WorkerThreads { get; set; } = Environment.ProcessorCount;

        public string PathMatch { get; set; } = "/discovery";

        public int DefaultRequestTimeout { get; set; } = 1000 * 100;
    }
}
