using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery
{
    public interface IDiscoveryClient
    {
        IList<IServiceInstance> ResolveInstances(string serviceName);

        Task RegisterAsync();

        Task DiscoverAsync();
    }
}
