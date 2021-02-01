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
                    if (peer != null)
                    {
                        var state = PeerState.Failed;
                        if (response != null)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                state = PeerState.Successful;
                            }
                            else
                            {
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.InternalServerError:
                                    case HttpStatusCode.BadGateway:
                                    case HttpStatusCode.ServiceUnavailable:
                                        state = PeerState.Failed;
                                        break;
                                    case HttpStatusCode.Forbidden:
                                    case HttpStatusCode.NotFound:
                                        //TODO
                                        break;
                                }
                            }
                        }
                        FreePeer(peer, state);
                    }
                    request.RequestUri = originUri;
                }
            });
        }

        private void FreePeer(UpstreamPeer peer, PeerState state)
        {
            lock (peer)
            {
                //peer.Connections--;
                if (state == PeerState.Successful)
                {
                    peer.Fails = 0;
                    return;
                }
                peer.Fails++;
                //强调失败刚刚发生，重置对该peer的冻结时间（Now - peer.Checked）
                peer.Checked = DateTime.Now;
                //随着失败次数逐渐接近MaxFails，权重平滑渐小
                //如果MaxFails为1，则会导致权重立即衰减至0
                if (peer.MaxFails > 0)
                {
                    peer.EffectiveWeight -= peer.Weight / peer.MaxFails;

                    if (peer.Fails >= peer.MaxFails)
                    {
                        _logger.LogDebug($"Upstream peer temporarily freezed.");
                    }
                }
                if (peer.EffectiveWeight < 0)
                {
                    peer.EffectiveWeight = 0;
                }
            }
        }
    }
}
