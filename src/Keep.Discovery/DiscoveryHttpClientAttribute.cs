using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscoveryHttpClientAttribute : Attribute
    {
        //public string BaseAddress { get; set; }

        public DiscoveryHttpClientAttribute() { }

        //public DiscoveryHttpClientAttribute(string baseAddress)
        //{
        //    BaseAddress = baseAddress;
        //}
    }
}
