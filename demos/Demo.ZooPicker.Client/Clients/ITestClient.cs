using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Client.Clients
{
    public interface ITestClient
    {
        Task<string> GetValueAsync();

        Task<User> GetUserByIdAsync(int id);
    }
}
