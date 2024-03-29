﻿using Keep.Http;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Discovery.Client.Clients
{
    public interface ITestApi
    {
        [Get("/users/{id}")]
        Task<User> GetUserAsync(int id);

        [Get("/test")]
        Task<string> GetAsync([Property(TimeoutHandler.TIMEOUT_KEY)] TimeSpan requestTimeout = default);
    }
}
