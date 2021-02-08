using Keep.Discovery.Contract;
using Keep.Discovery.LoadBalancer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Keep.Discovery.Tests
{
    public class LoadBalancerTests
    {
        [Fact]
        public void RoundRobin()
        {
            var instances = new List<IServiceInstance>
            {
                new ServiceInstance("test", 80) { Weight = 1 },
                new ServiceInstance("test", 81) { Weight = 2 },
                new ServiceInstance("test", 82) { Weight = 3 },
            };
            var record = new InstanceCacheRecord();
            foreach (var ins in instances)
            {
                record.InstanceMap.TryAdd(Guid.NewGuid(), ins);
            }
            var balancer = new RoundRobinBalancer(null, record);
            var ports = Enumerable.Range(0, 300).Select(_ => balancer.Pick().Instance.Port);
            {
                var x80 = ports.Count(p => p == 80);
                var x81 = ports.Count(p => p == 81);
                var x82 = ports.Count(p => p == 82);
                Assert.Equal(50, x80);
                Assert.Equal(100, x81);
                Assert.Equal(150, x82);
            }

            {
                var x80 = ports.Take(6).Count(p => p == 80);
                var x81 = ports.Take(6).Count(p => p == 81);
                var x82 = ports.Take(6).Count(p => p == 82);
                Assert.Equal(1, x80);
                Assert.Equal(2, x81);
                Assert.Equal(3, x82);
            }

            {
                var x80 = ports.Skip(60).Take(60).Count(p => p == 80);
                var x81 = ports.Skip(60).Take(60).Count(p => p == 81);
                var x82 = ports.Skip(60).Take(60).Count(p => p == 82);
                Assert.Equal(10, x80);
                Assert.Equal(20, x81);
                Assert.Equal(30, x82);
            }
        }

        [Fact]
        public void Random()
        {
            var instances = new List<IServiceInstance>
            {
                new ServiceInstance("test", 80) { Weight = 1 },
                new ServiceInstance("test", 81) { Weight = 2 },
                new ServiceInstance("test", 82) { Weight = 3 },
            };
            var record = new InstanceCacheRecord();
            foreach (var ins in instances)
            {
                record.InstanceMap.TryAdd(Guid.NewGuid(), ins);
            }

            var ports = default(IEnumerable<int>);

            var balancer = new RandomBalancer(null, record);
            {
                ports = Enumerable.Range(0, 600).Select(_ => balancer.Pick().Instance.Port).ToList();
                var x80 = ports.Count(p => p == 80);
                var x81 = ports.Count(p => p == 81);
                var x82 = ports.Count(p => p == 82);
                Assert.True(100 * 0.8 < x80 && x80 < 100 * 1.2);
                Assert.True(200 * 0.8 < x81 && x81 < 200 * 1.2);
                Assert.True(300 * 0.8 < x82 && x82 < 300 * 1.2);
            }

            (instances[0] as ServiceInstance).ServiceState = ServiceState.Down;
            {
                ports = Enumerable.Range(0, 600).Select(_ => balancer.Pick().Instance.Port).ToList();
                var x80 = ports.Count(p => p == 80);
                var x81 = ports.Count(p => p == 81);
                var x82 = ports.Count(p => p == 82);
                Assert.Equal(0, x80);
                Assert.True(240 * 0.8 < x81 && x81 < 240 * 1.2);
                Assert.True(360 * 0.8 < x82 && x82 < 360 * 1.2);
            }

            (instances[0] as ServiceInstance).ServiceState = ServiceState.Up;
            {
                ports = Enumerable.Range(0, 600).Select(_ => balancer.Pick().Instance.Port).ToList();
                var x80 = ports.Count(p => p == 80);
                var x81 = ports.Count(p => p == 81);
                var x82 = ports.Count(p => p == 82);
                Assert.True(100 * 0.8 < x80 && x80 < 100 * 1.2);
                Assert.True(200 * 0.8 < x81 && x81 < 200 * 1.2);
                Assert.True(300 * 0.8 < x82 && x82 < 300 * 1.2);
            }

            (instances[0] as ServiceInstance).ServiceState = ServiceState.Down;
            (instances[1] as ServiceInstance).ServiceState = ServiceState.Down;
            {
                ports = Enumerable.Range(0, 600).Select(_ => balancer.Pick().Instance.Port).ToList();
                var x80 = ports.Count(p => p == 80);
                var x81 = ports.Count(p => p == 81);
                var x82 = ports.Count(p => p == 82);
                Assert.Equal(0, x80);
                Assert.Equal(0, x81);
                Assert.Equal(600, x82);
            }
        }

        [Fact]
        public void Random_All_Down()
        {
            var instances = new List<IServiceInstance>
            {
                new ServiceInstance("test", 80) { Weight = 1, ServiceState=ServiceState.Down },
                new ServiceInstance("test", 81) { Weight = 2, ServiceState=ServiceState.Down },
                new ServiceInstance("test", 82) { Weight = 3, ServiceState=ServiceState.Down },
            };
            var record = new InstanceCacheRecord();
            foreach (var ins in instances)
            {
                record.InstanceMap.TryAdd(Guid.NewGuid(), ins);
            }

            var balancer = new RandomBalancer(null, record);
            Assert.Null(balancer.Pick());
        }
    }
}
