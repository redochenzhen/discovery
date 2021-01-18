using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Keep.Discovery.Contract;
using System.Text.Json;

namespace Keep.Discovery.Internal
{
    public class InstanceQueryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly InstanceCache _instanceRegistry;
        private readonly DiscoveryOptions _options;

        public InstanceQueryMiddleware(
            RequestDelegate next,
            IOptions<DiscoveryOptions> discoveryOptions,
            InstanceCache instanceRegistry)
        {
            _next = next;
            _instanceRegistry = instanceRegistry ?? throw new ArgumentNullException(nameof(instanceRegistry));
            _options = discoveryOptions?.Value ?? throw new ArgumentNullException(nameof(discoveryOptions));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments(_options.PathMatch))
            {
                await _next(context);
                return;
            }
            var mapping = _instanceRegistry.GetAll()
                .Select(kv => new StaticServiceEntry
                {
                    ServiceName = kv.Key,
                    Instances = kv.Value.Values
                    .Select(si => new InstanceEntry
                    {
                        Host = si.HostName,
                        Port = si.Port,
                        Secure = si.IsSecure,
                        State = si.ServiceState,
                        Weight = si.Weight
                    })
                    .ToList()
                })
                .ToList();
            var result = new
            {
                Mapping = mapping
            };
            var option = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            };
            var json = JsonSerializer.Serialize(result, option);
            await context.Response.WriteAsync(json);
        }
    }

    internal sealed class StartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var sp = app.ApplicationServices;
                var discoveryOptions = sp.GetService<IOptions<DiscoveryOptions>>();
                if (discoveryOptions?.Value.ShouldDiscover ?? false)
                {
                    app.UseMiddleware<InstanceQueryMiddleware>();
                }
                next(app);
            };
        }
    }
}
