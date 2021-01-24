using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery
{
    public interface IDiscoveryClient
    {
        Task RegisterAsync();

        Task DiscoverAsync();
    }
}
