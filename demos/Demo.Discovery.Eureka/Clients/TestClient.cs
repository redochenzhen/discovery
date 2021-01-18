using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Demo.Discovery.Eureka.Clients
{
    class TestClient : ITestClient
    {
        private readonly HttpClient _http;

        public TestClient(HttpClient http)
        {
            _http = http;
        }

        public string GetValue(string value)
        {
            var r = _http.GetStringAsync("http://x1/api/value").Result;
            return r;
        }
    }
}
