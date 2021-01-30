using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    internal class StaticInstanceEntry
    {
        public string ServiceName { get; set; }
        public List<InstanceEntry> Instances { get; set; }
    }
}
