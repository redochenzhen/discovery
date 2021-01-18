using Keep.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Demo.Discovery.ZooPicker.C.Clients
{
    [DiscoveryHttpClient]
    class TestClient : ITestClient
    {
        private readonly HttpClient _http;

        public TestClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetValueAsync()
        {
            var value = await _http.GetStringAsync("/test");
            return $"From Discovery: {value}";
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var response = await _http.GetAsync($"/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new NotSupportedException();
            }
            var user = await response.Content.ReadAsAsync<User>();
            return user;
        }
    }
}
