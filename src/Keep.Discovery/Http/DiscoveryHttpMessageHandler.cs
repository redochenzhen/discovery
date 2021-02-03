using Keep.Common.Http;
using Keep.Discovery.Internal;
using Keep.Discovery.Pump;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Keep.Discovery.Refit")]

namespace Keep.Discovery.Http
{
    internal class DiscoveryHttpMessageHandler : TimeoutHandler
    {
        private readonly ILogger _logger;
        private readonly IDispatcher _dispather;

        public DiscoveryHttpMessageHandler(
            ILogger<DiscoveryHttpMessageHandler> logger,
            IDispatcher dispather)
        {
            _logger = logger;
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
            try
            {
                await _dispather.AcceptThenDispatchAsync(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                ctx.ResponsSource.SetException(ex);
            }
            return await ctx.ResponsSource.Task;
        }
    }
}
