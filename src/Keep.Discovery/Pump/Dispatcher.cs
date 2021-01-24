using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Keep.Discovery.Contract;
using Keep.Discovery.Internal;
using Keep.Discovery.LoadBalancer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Keep.Discovery.Pump
{
    internal class Dispatcher : IDispatcher
    {
        private readonly CancellationTokenSource _cts;
        private readonly BufferBlock<HandlingContext> _requestBuffer;
        private readonly ILogger _logger;
        private readonly DiscoveryOptions _options;
        private readonly IDiscoveryClient _discoveryClient;

        private readonly IBalancerFactory _balancerFactory;
        private readonly ThreadLocal<Dictionary<string, IBalancer>> _balancers;
        private readonly object _lock;

        public Dispatcher(
            ILogger<Dispatcher> logger,
            IOptions<DiscoveryOptions> options,
            IDiscoveryClient discoveryClient,
            IBalancerFactory balancerFactory)
        {
            _cts = new CancellationTokenSource();
            var dbOption = new DataflowBlockOptions
            {
                CancellationToken = _cts.Token
            };
            _requestBuffer = new BufferBlock<HandlingContext>(dbOption);

            _logger = logger;
            _options = options.Value;
            _discoveryClient = discoveryClient;
            _balancerFactory = balancerFactory;
            _balancers = new ThreadLocal<Dictionary<string, IBalancer>>();
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
                var tcs = ctx.ResponsSource;
                var request = ctx.Request;
                var current = request.RequestUri;
                string originalScheme = current.Scheme;
                string serviceName = current.Host;
                try
                {
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

                    var instance = balancer.Pick();
                    var uri = instance?.Uri;
                    if (uri != null)
                    {
                        request.RequestUri = new Uri(uri, current.PathAndQuery);
                    }
                    Task.Run(async () =>
                    {
                        try
                        {
                            var response = await ctx.HandleAsync(request, ctx.CancellationToken);
                            ctx.ResponsSource.TrySetResult(response);
                        }
                        catch (Exception ex)
                        {
                            ctx.ResponsSource.TrySetException(ex);
                        }
                    });
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex.InnerException ?? ex);
                }
            }
        }

        private void LongRun(Action action)
        {
            Task.Factory.StartNew(action, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
