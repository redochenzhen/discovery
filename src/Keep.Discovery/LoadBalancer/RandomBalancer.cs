using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Keep.Discovery.Tests")]

namespace Keep.Discovery.LoadBalancer
{
    internal class RandomBalancer : IBalancer
    {
        private readonly object _lock = new object();
        private readonly Random _random = new Random(42);

        public IServiceInstance Pick()
        {
            throw new NotImplementedException();
            //if (instances == null) return null;
            //int count = instances.Count;
            //if (count <= 1) return instances.FirstOrDefault();
            //lock (_lock)
            //{
            //    int idx = _random.Next(count);
            //    return instances[idx];
            //}
        }

        public void SetInstances(IList<IServiceInstance> instances)
        {
            throw new NotImplementedException();
        }
    }
}
