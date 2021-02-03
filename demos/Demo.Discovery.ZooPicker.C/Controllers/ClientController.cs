using Demo.Discovery.ZooPicker.C.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Demo.Discovery.ZooPicker.C.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITestClient _testClient;
        private readonly ITestApi _testApi;

        public ClientController(ILogger<ClientController> logger, ITestClient client, ITestApi api)
        {
            _logger = logger;
            _testClient = client;
            _testApi = api;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var x = await _testClient.GetValueAsync();
            //var x = await _testApi.GetAsync();
            return Ok(x);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            //var user = await _testClient.GetUserByIdAsync(id);
            var user = await _testApi.GetUserAsync(id);
            user.Mark = "client";
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
