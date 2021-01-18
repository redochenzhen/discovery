using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    public interface IBalancer
    {
        IServiceInstance PickOne(IList<IServiceInstance> intances);
    }
}
