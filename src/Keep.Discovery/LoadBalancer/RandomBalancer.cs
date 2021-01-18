using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal class RandomBalancer : IBalancer
    {
        public IServiceInstance PickOne(IList<IServiceInstance> instances)
        {
            if (instances == null) return null;
            int count = instances.Count;
            if (count <= 1) return instances.FirstOrDefault();
            var x = new Random();
            int idx = x.Next(0, count - 1);
            return instances[idx];
        }
    }
}
