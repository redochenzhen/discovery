using Keep.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery
{
    public class DiscoveryOptions : OptionsBase
    {
        public const string CONFIG_PREFIX = "discovery";

        public bool ShouldDiscover { get; set; } = false;

        public bool ShouldRegister { get; set; } = false;

        public string PathMatch { get; set; } = "/discovery";
    }
}
