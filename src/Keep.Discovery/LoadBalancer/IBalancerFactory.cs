using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal interface IBalancerFactory
    {
        IBalancer CreateBalancer(string serviceName);
    }
}
