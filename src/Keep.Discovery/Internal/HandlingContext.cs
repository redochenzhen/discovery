using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keep.Discovery.Internal
{
    internal class HandlingContext
    {
        public TaskCompletionSource<HttpResponseMessage> ResponsSource { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public HttpRequestMessage Request { get; set; }

        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> HandleAsync;
    }
}
