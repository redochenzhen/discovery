﻿using Keep.Discovery.Contract;
using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Keep.Discovery.Pump;
using Microsoft.Extensions.Logging;
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

        public HttpUpstreamHandler(ILogger<HttpUpstreamHandler> logger)
        {
            _logger = logger;
        }

        public void SetDispatcher(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Handle(
            HandlingContext handlingContext,
            UpstreamPeer peer)
        {
            //负载均衡完成选择之后，由线程池线程完成接下来的http请求处理
            Task.Run(async () =>
            {
                var request = handlingContext.Request;
                var tcs = handlingContext.ResponsSource;
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
                    var ct = handlingContext.CancellationToken;
                    response = await handlingContext.HandleAsync(request, ct);
                    if (!await NextPeerAsync(handlingContext, peer, response, originalUri))
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
                    if (!await NextPeerAsync(handlingContext, peer, response, originalUri, true))
                    {
                        tcs.SetException(ex);
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (!await NextPeerAsync(handlingContext, peer, response, originalUri))
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
                    FreePeer(handlingContext, peer, response);
                }
            });
        }

        /// <returns>是否尝试了“下一个”上游服务端</returns>
        private async Task<bool> NextPeerAsync(
            HandlingContext handlingContext,
            UpstreamPeer peer,
            HttpResponseMessage response,
            Uri originUri,
            bool isTimeout = false)
        {
            if (peer == null) return false;
            if (handlingContext.IsSingle) return false;

            handlingContext.Start = handlingContext.Start ?? DateTime.Now;

            if (NextWhen.Never == (peer.NextWhen & NextWhen.Never)) return false;

            var request = handlingContext.Request;
            if (request.Method != HttpMethod.Get &&
                NextWhen.GetOnly == (peer.NextWhen & NextWhen.GetOnly))
            {
                return false;
            }

            handlingContext.Tries = handlingContext.Tries ?? (peer.NextTries == 0 ? int.MaxValue : peer.NextTries);
            if (handlingContext.Tries == 0) return false;

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

            if (DateTime.Now - handlingContext.Start >= peer.NextTimeout) return false;

            handlingContext.Request = request.Clone(originUri);
            //保持上下文重试
            var next = await _dispatcher.AcceptThenDispatchAsync(handlingContext);

            handlingContext.Tries--;
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
                    //上次失败后已经过了至少一个Check
                    if (peer.LastFailed < peer.Checked)
                    {
                        peer.Fails = 0;
                    }
                    return;
                }
                peer.Fails++;
                //失败刚刚发生，重置对该peer的冻结判定时间（now - Checked <= FailTimeout）
                peer.Checked = DateTime.Now;
                peer.LastFailed = peer.Checked;
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
