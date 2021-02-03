using System;
using System.Collections;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Keep.Discovery.Internal
{
    /// <summary>
    /// 处理http请求的上下文
    /// </summary>
    internal class HandlingContext
    {
        //发http请求所必要的元素-------------------
        public TaskCompletionSource<HttpResponseMessage> ResponsSource { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public HttpRequestMessage Request { get; set; }

        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> HandleAsync;
        //-------------------------------------

        /// <summary>
        /// 已尝试的次数
        /// </summary>
        public int? Tries { get; set; }

        /// <summary>
        /// 开始尝试的时间
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        /// 已尝试标记（基于索引）
        /// </summary>
        public BitArray TriedMark { get; set; }

        /// <summary>
        /// 上游端版本号
        /// </summary>
        public int PeersVersion { get; set; }

        /// <summary>
        /// 是否只有单个上游端
        /// </summary>
        public bool IsSingle => TriedMark?.Length == 1;
    }
}
