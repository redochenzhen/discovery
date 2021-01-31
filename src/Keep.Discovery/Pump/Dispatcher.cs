using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
        private readonly BufferBlock<HandlingContext> _requestBuffer;
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
            var dbOption = new DataflowBlockOptions
            {
                CancellationToken = _cts.Token
            };
            _requestBuffer = new BufferBlock<HandlingContext>(dbOption);

            _logger = logger;
            _options = options.Value;
            _balancers = new ThreadLocal<Dictionary<string, IBalancer>>();
            _balancerFactory = balancerFactory;
            _handler = handler;
        }


        public async Task<bool> AcceptThenDispatchAsync(HandlingContext context)
        {
            return await _requestBuffer.SendAsync(context, _cts.Token);
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

        private void Handling()
        {
            //这里使用同步调用，确保一直使用long-running线程
            while (_requestBuffer.OutputAvailableAsync(_cts.Token).Result)
            {
                var ctx = _requestBuffer.ReceiveAsync().Result;
                string serviceName = ctx.Request.RequestUri.Host;

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
                var peer = balancer.Pick();
                _handler.Handle(ctx, peer);
            }
        }

        private void LongRun(Action action)
        {
            Task.Factory.StartNew(action, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
