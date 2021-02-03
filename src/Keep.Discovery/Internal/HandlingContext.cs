using System;
using System.Collections;
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

        //-------------------------------------

        public int? Tries { get; set; }

        public DateTime? Start { get; set; }

        public BitArray TriedMark { get; set; }

        public int PeersVersion { get; set; }

        public int PeersCount { get; set; }
    }
}
