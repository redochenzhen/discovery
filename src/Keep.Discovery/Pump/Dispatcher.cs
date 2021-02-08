using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Keep.Discovery.Http;
using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Keep.Discovery.Pump
{
    internal class Dispatcher : IDispatcher
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts;
        private readonly Channel<HandlingContext> _requestBuffer;
        private readonly DiscoveryOptions _options;
        private readonly ThreadLocal<Dictionary<string, IBalancer>> _balancers;
        private readonly IBalancerFactory _balancerFactory;
        private readonly HttpUpstreamHandler _handler;


        public Dispatcher(
            ILogger<Dispatcher> logger,
            IOptions<DiscoveryOptions> options,
            IBalancerFactory balancerFactory,
            HttpUpstreamHandler handler)
        {
            _cts = new CancellationTokenSource();
            _requestBuffer = Channel.CreateUnbounded<HandlingContext>();
            _logger = logger;
            _options = options.Value;
            _balancers = new ThreadLocal<Dictionary<string, IBalancer>>();
            _balancerFactory = balancerFactory;
            _handler = handler;
            handler.SetDispatcher(this);
        }

        public async Task<bool> AcceptThenDispatchAsync(HandlingContext context)
        {
            var writer = _requestBuffer.Writer;
            await writer.WriteAsync(context);
            return true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public void Pumping()
        {
            for (int i = 0; i < _options.WorkerThreads; i++)
            {
                LongRun(Handling);
            }
        }

        //该方法中不宜使用异步编程，确保一直处于long-running线程中
        //否则将对线程池造成额外负担，且浪费不必要的ThreadLocal Balancers
        private void Handling()
        {
            var reader = _requestBuffer.Reader;
#if DEBUG
            if (_options.WorkerThreads == 1)
            {
                _logger.LogDebug($"Should alway be in the same thread: {Thread.CurrentThread.ManagedThreadId}.");
            }
#endif
            while (reader.WaitToReadAsync(_cts.Token).AsTask().Result)
            {
#if DEBUG
                if (_options.WorkerThreads == 1)
                {
                    _logger.LogDebug($"Should be alway in the same thread: {Thread.CurrentThread.ManagedThreadId}.");
                }
#endif
                while (reader.TryRead(out var handlingCtx))
                {
                    string serviceName = handlingCtx.Request.RequestUri.Host;
                    var balancerMap = _balancers.Value;
                    if (balancerMap == null)
                    {
                        balancerMap = new Dictionary<string, IBalancer>();
                        _balancers.Value = balancerMap;
                    }
                    if (!balancerMap.TryGetValue(serviceName, out var balancer))
                    {
                        balancer = _balancerFactory.CreateBalancer(serviceName);
                        balancerMap.Add(serviceName, balancer);
                    }
                    //如果这是一个重试请求，上下文会携带tried标记，避开已经尝试过的peer
                    balancer.TriedMark = handlingCtx.TriedMark ?? new BitArray(balancer.PeersCount);
                    var peer = balancer.Pick();
                    handlingCtx.TriedMark = balancer.TriedMark;
                    handlingCtx.PeersVersion = balancer.PeersVersion;
                    _handler.Handle(handlingCtx, peer);
                }
            }
        }

        private void LongRun(Action action)
        {
            Task.Factory.StartNew(action, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
