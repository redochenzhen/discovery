using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "!!!";
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            //await Task.Delay(500);
            var data = new List<User>
            {
                new User{ Id = 1, Name = "Jim", Age = 17 },
                new User{ Id = 2, Name = "Tom", Age = 18 }
            };

            var user = data.Where(u => u.Id == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }

    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Mark { get; set; } = "server";
    }
}
