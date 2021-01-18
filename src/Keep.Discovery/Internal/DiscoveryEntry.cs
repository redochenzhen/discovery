using Keep.Discovery.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keep.Discovery.Internal
{
    internal class DiscoveryEntry : BackgroundService, IDiscoveryEntry
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IWorker> _workers;

        public DiscoveryEntry(
            ILogger<DiscoveryEntry> logger,
            IEnumerable<IWorker> workers)
        {
            _logger = logger;
            _workers = workers;
        }

        public void Boot(CancellationToken stoppingToken)
        {
            _logger.LogDebug("*** Discovery service is booting.");
            stoppingToken.Register(() =>
            {
                _logger.LogDebug("*** Discovery service is stopping");
                foreach (var worker in _workers)
                {
                    try
                    {
                        worker.Stop();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            foreach (var worker in _workers)
            {
                try
                {
                    worker.Start();
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Boot(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
