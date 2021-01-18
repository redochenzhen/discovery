using Keep.Discovery.LoadBalancer;
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
        private readonly IDiscoveryClient _discoveryClient;
        private readonly DiscoveryOptions _options;
        private readonly IBalancer _balancer;

        public DiscoveryHttpMessageHandler(
            IOptions<DiscoveryOptions> options,
            IDiscoveryClient discoveryClient,
            IBalancer balancer)
        {
            _options = options.Value;
            _discoveryClient = discoveryClient;
            _balancer = balancer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            string originalScheme = current.Scheme;
            string serviceName = current.Host;
            var instances = _discoveryClient.ResolveInstances(serviceName);
            var instance = _balancer.PickOne(instances);
            var uri = instance?.Uri;
            if (uri != null)
            {
                request.RequestUri = new Uri(uri, current.PathAndQuery);
            }
            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return response;
            }
            finally
            {
                request.RequestUri = current;
            }
        }
    }
}
