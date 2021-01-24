using Keep.Discovery.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.LoadBalancer
{
    internal class BalancerFactory : IBalancerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly InstanceCache _instanceCache;

        public BalancerFactory(
            IServiceProvider serviceProvider,
            InstanceCache instanceCache)
        {
            _serviceProvider = serviceProvider;
            _instanceCache = instanceCache;
        }

        public IBalancer CreateBalancer(string serviceName)
        {
            var record = _instanceCache.GetCacheRecord(serviceName);
            var policy = record?.Policy ?? BalancePolicy.RoundRobin;
            if (policy == BalancePolicy.RoundRobin)
            {
                var logger = _serviceProvider.GetService<ILogger<RoundRobinBalancer>>();
                var rrBalancer = new RoundRobinBalancer(logger, record);
                return rrBalancer;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
