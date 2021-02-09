using System.Text.Json.Serialization;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 服务实例实体（Json序列化后存储于ZooKeeper节点）
    /// </summary>
    public class InstanceEntry
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType Type { get; set; }

        /// <summary>
        /// 服务主机名称（或IP地址）
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否https
        /// </summary>
        public bool Secure { get; set; } = false;

        /// <summary>
        /// 服务状态
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceState State { get; set; }

        /// <summary>
        /// 负载均衡策略
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BalancePolicy Balancing { get; set; }

        /// <summary>
        /// 负载均衡权重
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 失败超时时间
        /// </summary>
        public int FailTimeout { get; set; }

        /// <summary>
        /// 失败超时时间内，允许发生的最大失败次数
        /// </summary>
        public int MaxFails { get; set; }

        /// <summary>
        /// 当Http请求发生配置所述状况时，将选择另一个实例重试
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NextWhen NextWhen { get; set; }

        /// <summary>
        /// 限制重试次数
        /// </summary>
        public int NextTries { get; set; }

        /// <summary>
        /// 限制重试时间
        /// </summary>
        public int NextTimeout { get; set; }
    }
}
