using System;
using System.Collections;

namespace Keep.Discovery.LoadBalancer
{
    internal interface IBalancer
    {
        /// <summary>
        /// 上游服务端版本号，当其值与CacheVersion不一致时，需要重置上游服务端
        /// </summary>
        int PeersVersion { get; }

        int PeersCount { get; }

        /// <summary>
        /// 已尝试上游服务端标记（基于索引）
        /// </summary>
        /// <remarks>比如BitArray{00000110}表示上游服务端列表中的第1、2项已经被尝试过</remarks>
        BitArray TriedMark { get; set; }

        /// <summary>
        /// 选择一个最佳的上游服务端，找不到满足条件的服务则返回null
        /// </summary>
        UpstreamPeer Pick();
    }
}
