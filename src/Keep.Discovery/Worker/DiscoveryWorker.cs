using Keep.Discovery.Exceptions;
using Keep.Discovery.Pump;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keep.Discovery.Worker
{
    internal class DiscoveryWorker : IWorker
    {
        private readonly ILogger _logger;
        private readonly DiscoveryOptions _options;
        private readonly IDiscoveryClient _discoveryClient;
        private bool _isHealthy = true;
        private CancellationTokenSource _cts;
        private readonly IDispatcher _dispather;

        public DiscoveryWorker(
            ILogger<DiscoveryWorker> logger,
            IOptions<DiscoveryOptions> options,
            IDiscoveryClient discoveryClient,
            IDispatcher dispather)
        {
            _logger = logger;
            _options = options.Value;
            _discoveryClient = discoveryClient;
            _dispather = dispather;
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            if (!_options.ShouldDiscover) return;
            _logger.LogDebug("Discovery worker is starting.");

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await _discoveryClient.DiscoverAsync();
                }
                catch (UnhealthyException ex)
                {
                    _isHealthy = false;
                    _logger.LogError(ex.InnerException ?? ex, ex.Message);
                    Restart();
                }
                catch (TimeoutException ex)
                {
                    _isHealthy = false;
                    _logger.LogError(ex.Message);
                    await Task.Delay(1000 * 30);
                    Restart();
                }
            }, _cts.Token);

            _dispather.Pumping();
        }

        public void Pulse()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public void Restart(bool force = false)
        {
            _logger.LogDebug("Discovery worker is restarting.");
            if (!_isHealthy || force)
            {
                Pulse();
                _cts = new CancellationTokenSource();
                _isHealthy = true;
                Start();
            }
        }

        public void Stop()
        {
            _logger.LogDebug("Discovery worker is stopping.");
        }
    }
}
