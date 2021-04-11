using Keep.Framework;
using System;

namespace Keep.Discovery
{
    /// <summary>
    /// 服务发现选项
    /// </summary>
    public class DiscoveryOptions : OptionsBase
    {
        public const string CONFIG_PREFIX = "discovery";

        /// <summary>
        /// 是否启用发现功能
        /// </summary>
        public bool ShouldDiscover { get; set; } = false;

        /// <summary>
        /// 是否启用注册功能
        /// </summary>
        public bool ShouldRegister { get; set; } = false;

        /// <summary>
        /// 处理Http请求的线程数，默认值为cpu核数
        /// </summary>
        public int WorkerThreads { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// 服务实例信息查询的路由配置
        /// </summary>
        public string PathMatch { get; set; } = "/discovery";

        /// <summary>
        /// Http请求默认超时时间（有别于HttpClient.Timeout）
        /// </summary>
        public int DefaultRequestTimeout { get; set; } = 1000 * 100;
    }
}
