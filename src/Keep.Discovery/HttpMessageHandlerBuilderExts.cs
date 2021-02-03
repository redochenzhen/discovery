using Keep.Discovery.Http;
using Microsoft.Extensions.Http;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpMessageHandlerBuilderExts
    {
        public static HttpMessageHandlerBuilder SetDefaultTimeout(this HttpMessageHandlerBuilder builder, int timeoutMilliseconds)
        {
            return SetDefaultTimeout(builder, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        public static HttpMessageHandlerBuilder SetDefaultTimeout(this HttpMessageHandlerBuilder builder, TimeSpan timeoutTimeSpan)
        {
            if (builder.AdditionalHandlers?.Count > 0)
            {
                var discoveryHander = builder.AdditionalHandlers
                    .Cast<DiscoveryHttpMessageHandler>()
                    .FirstOrDefault(h => h != null);
                if (discoveryHander != null)
                {
                    discoveryHander.DefaultTimeout = timeoutTimeSpan;
                }
            }
            return builder;
        }
    }
}
