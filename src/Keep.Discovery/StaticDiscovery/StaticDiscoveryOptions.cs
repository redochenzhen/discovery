using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.StaticDiscovery
{
    internal class StaticDiscoveryOptions
    {
        public const string CONFIG_PREFIX = "discovery";

        public List<StaticServiceEntry> Mapping { get; set; }
    }
}
