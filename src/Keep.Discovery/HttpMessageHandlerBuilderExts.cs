using Keep.Discovery.Http;
using Microsoft.Extensions.Http;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpMessageHandlerBuilderExts
    {
        /// <summary>
        /// 设置默认请求超时时间
        /// </summary>
        /// <param name="builder">HttpMessageHandler配置器</param>
        /// <param name="timeoutMilliseconds">超时毫秒数</param>
        /// <returns></returns>
        public static HttpMessageHandlerBuilder SetDefaultTimeout(this HttpMessageHandlerBuilder builder, int timeoutMilliseconds)
        {
            return SetDefaultTimeout(builder, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// 设置默认请求超时时间
        /// </summary>
        /// <param name="builder">HttpMessageHandler配置器</param>
        /// <param name="timeoutTimeSpan">超时时间</param>
        /// <returns></returns>
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
