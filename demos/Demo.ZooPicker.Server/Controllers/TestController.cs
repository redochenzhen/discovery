using Keep.Discovery.ZooPicker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        IOptions<ZooPickerOptions> _options;
        public TestController(ILogger<TestController> logger,IOptions<ZooPickerOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        [HttpGet]
        public string Get()
        {
            return _options.Value.Instance.IpAddress;
        }
    }
}
