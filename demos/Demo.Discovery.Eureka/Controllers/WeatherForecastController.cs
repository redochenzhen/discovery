using Demo.Discovery.Eureka.Clients;
using Keep.Discovery.Eureka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Eureka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger _logger;
        private readonly ITestClient _c;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ITestClient c)
        {
            _logger = logger;
            _c = c;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var s = _c.GetValue("a");
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
