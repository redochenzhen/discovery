using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery.Http
{
    internal class HttpUpstreamHandler
    {
        private readonly ILogger _logger;

        public HttpUpstreamHandler(ILogger<HttpUpstreamHandler> logger)
        {
            _logger = logger;
        }

        public void Handle(
            HandlingContext context,
            UpstreamPeer peer)
        {
            //负载均衡完成选择之后，由线程池线程完成接下来的http请求处理
            Task.Run(async () =>
            {
                var request = context.Request;
                var tcs = context.ResponsSource;
                var originUri = request.RequestUri;
                //string originalScheme = current.Scheme;
                var response = default(HttpResponseMessage);
                try
                {
                    var instance = peer?.Instance;
                    var uri = instance?.Uri;
                    if (uri != null)
                    {
                        request.RequestUri = new Uri(uri, originUri.PathAndQuery);
                    }
                    var ct = context.CancellationToken;
                    response = await context.HandleAsync(request, ct);
                    tcs.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    if (peer != null && response != null)
                    {
                        FreePeer(peer, response.StatusCode);
                    }
                    request.RequestUri = originUri;
                }
            });
        }

        private void FreePeer(UpstreamPeer peer, HttpStatusCode statusCode)
        {
        }
    }
}
