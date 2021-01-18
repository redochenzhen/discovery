using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Eureka.Clients
{
    public interface ITestClient
    {
        string GetValue(string value);
    }
}
