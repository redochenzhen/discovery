using Demo.Discovery.ZooPicker.C.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Demo.Discovery.ZooPicker.C
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(options =>
            {
                options.AddConsole();
                //options.AddFile("logs/{Date}.log", LogLevel.Debug);
            });
            services
                .AddDiscovery(options =>
                {
#if DEBUG
                    options.UseZooPicker();
#else
                    options.UseStaticMapping();
#endif
                })
                .AddDiscoveryHttpClient<ITestClient, TestClient>()
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    builder.SetDefaultTimeout(1000);
                })
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("http://testapi");
                });
            services
                .AddDiscoveryRefitClient<ITestApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("http://testapi");
                });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
