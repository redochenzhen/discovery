using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Keep.Discovery.Pump;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Keep.Discovery.Refit")]

namespace Keep.Discovery.Http
{
    internal class DiscoveryHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        private readonly DiscoveryOptions _options;
        private readonly IDispatcher _dispather;

        public DiscoveryHttpMessageHandler(
            ILogger<DiscoveryHttpMessageHandler> logger,
            IOptions<DiscoveryOptions> options,
            IDispatcher dispather)
        {
            _logger = logger;
            _options = options.Value;
            _dispather = dispather;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = new HandlingContext
            {
                Request = request,
                HandleAsync = base.SendAsync,
                ResponsSource = new TaskCompletionSource<HttpResponseMessage>(cancellationToken),
                CancellationToken = cancellationToken
            };
            await _dispather.AcceptThenDispatchAsync(ctx);
            return await ctx.ResponsSource.Task;
        }
    }
}
