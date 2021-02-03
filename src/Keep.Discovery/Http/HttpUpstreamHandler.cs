using Keep.Discovery.Contract;
using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Keep.Discovery.Pump;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Keep.Discovery.Http
{
    internal class HttpUpstreamHandler
    {
        private readonly ILogger _logger;
        private IDispatcher _dispatcher;
        private readonly IOptions<DiscoveryOptions> _options;

        public HttpUpstreamHandler(ILogger<HttpUpstreamHandler> logger)
        {
            _logger = logger;
        }

        public void SetDispatcher(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
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
                var originalUri = request.RequestUri;
                //string originalScheme = current.Scheme;
                var response = default(HttpResponseMessage);
                try
                {
                    var instance = peer?.Instance;
                    var uri = instance?.Uri;
                    if (uri != null)
                    {
                        request.RequestUri = new Uri(uri, originalUri.PathAndQuery);
                        lock (peer)
                        {
                            peer.Connections++;
                        }
                    }
                    var ct = context.CancellationToken;
                    response = await context.HandleAsync(request, ct);
                    if (!await NextPeerAsync(context, peer, response, originalUri))
                    {
                        tcs.TrySetResult(response);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    tcs.SetException(ex);
                    _logger.LogWarning(ex, "Ensure that this cancelation is not caused by timeout, or you may lost the chance to try another upstream peer.");
                    return;
                }
                catch (TimeoutException ex)
                {
                    if (!await NextPeerAsync(context, peer, response, originalUri, true))
                    {
                        tcs.SetException(ex);
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (!await NextPeerAsync(context, peer, response, originalUri))
                    {
                        tcs.TrySetException(ex);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    tcs.SetException(ex);
                }
                finally
                {
                    FreePeer(context, peer, response);
                }
            });
        }

        /// <returns>是否尝试了“下一个”上游服务端</returns>
        private async Task<bool> NextPeerAsync(
            HandlingContext context,
            UpstreamPeer peer,
            HttpResponseMessage response,
            Uri originUri,
            bool isTimeout = false)
        {
            if (peer == null) return false;
            if (context.IsSingle) return false;

            context.Start = context.Start ?? DateTime.Now;

            if (NextWhen.Never == (peer.NextWhen & NextWhen.Never)) return false;

            var request = context.Request;
            if (request.Method != HttpMethod.Get &&
                NextWhen.GetOnly == (peer.NextWhen & NextWhen.GetOnly))
            {
                return false;
            }

            context.Tries = context.Tries ?? (peer.NextTries == 0 ? int.MaxValue : peer.NextTries);
            if (context.Tries == 0) return false;

            var when = NextWhen.None;
            if (isTimeout)
            {
                when |= NextWhen.Timeout;
            }
            else if (response == null)
            {
                when |= NextWhen.Error;
            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    return false;
                }
                switch (response.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        when |= NextWhen.Http500;
                        break;
                    case HttpStatusCode.BadGateway:
                        when |= NextWhen.Http502;
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        when |= NextWhen.Http503;
                        break;
                    case HttpStatusCode.RequestTimeout:
                    case HttpStatusCode.GatewayTimeout:
                        when |= NextWhen.Timeout;
                        break;
                    case HttpStatusCode.Forbidden:
                        when |= NextWhen.Http403;
                        break;
                    case HttpStatusCode.NotFound:
                        when |= NextWhen.Http404;
                        break;
                    default:
                        return false;
                }
            }

#if NETSTANDARD2_0
            if (request.Method == HttpMethod.Post || request.Method.Method == "PATCH")
#elif NETSTANDARD2_1
            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Patch)
#endif
            {
                when |= NextWhen.NonIdemponent;
            }

            if (when != (when & peer.NextWhen)) return false;

            if (DateTime.Now - context.Start >= peer.NextTimeout) return false;

            context.Request = request.Clone(originUri);
            var next = await _dispatcher.AcceptThenDispatchAsync(context);

            context.Tries--;
            return next;
        }

        private void FreePeer(
            HandlingContext context,
            UpstreamPeer peer,
            HttpResponseMessage response)
        {
            if (peer == null) return;
            if (context.IsSingle) return;

            PeerState state = PeerState.Successful;
            if (response == null)
            {
                state = PeerState.Failed;
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
                    default:
                        break;
                }
            }
            lock (peer)
            {
                peer.Connections--;
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
