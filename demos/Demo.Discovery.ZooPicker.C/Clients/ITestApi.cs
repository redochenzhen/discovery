using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.ZooPicker.C.Clients
{
    public interface ITestApi
    {
        [Get("/users/{id}")]
        Task<User> GetUserAsync(int id);
    }
}
