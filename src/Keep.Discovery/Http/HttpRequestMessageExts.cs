using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Keep.Discovery.Http
{
    internal static class HttpRequestMessageExts
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request, Uri requestUri = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var newReq = new HttpRequestMessage(request.Method, requestUri ?? request.RequestUri);
            foreach (var h in request.Headers)
            {
                newReq.Headers.Add(h.Key, h.Value);
            }
            foreach (var p in request.Properties)
            {
                newReq.Properties.Add(p.Key, p.Value);
            }
            newReq.Content = request.Content;
            newReq.Version = request.Version;
            return newReq;
        }
    }
}
