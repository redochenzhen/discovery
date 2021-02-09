using Keep.Discovery.Contract;

namespace Keep.Discovery.ZooPicker
{
    /// <summary>
    /// ZooPicker选项
    /// </summary>
    public class ZooPickerOptions
    {
        public const string CONFIG_PREFIX = "discovery:zoopicker";

        /// <summary>
        /// ZooKeeper服务连接字符串（ZooPicker服务发现的跟节点）
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// ZooKeeper会话超时时间，默认值为6,000ms
        /// </summary>
        public int SessionTimeout { get; set; } = 2000 * 3;

        /// <summary>
        /// ZooKeeper连接超时时间，默认值为20,000ms
        /// </summary>
        public int ConnectionTimeout { get; set; } = 1000 * 20;

        /// <summary>
        /// 服务分组（用于组织管理服务），默认值为"Default"
        /// </summary>
        public string GroupName { get; set; } = "Default";

        /// <summary>
        /// 服务实例选项信息
        /// </summary>
        public InstanceOptions Instance { get; set; } = new InstanceOptions();

        /// <summary>
        /// 服务实例选项
        /// </summary>
        public class InstanceOptions
        {
            /// <summary>
            /// 服务名称
            /// </summary>
            public string ServiceName { get; set; }

            /// <summary>
            /// 服务类型，默认值为Rest
            /// </summary>
            public ServiceType Type { get; set; } = ServiceType.Rest;

            /// <summary>
            /// 服务端口
            /// </summary>
            public int Port { get; set; } = 0;

            /// <summary>
            /// 是否使用https
            /// </summary>
            public bool Secure { get; set; } = false;

            /// <summary>
            /// 负载均衡权重，默认值为1
            /// </summary>
            public int Weight { get; set; } = 1;

            /// <summary>
            /// 服务状态
            /// </summary>
            public ServiceState State { get; set; } = ServiceState.Up;

            /// <summary>
            /// 负载均衡策略，默认值为RoundRobin
            /// </summary>
            public BalancePolicy BalancePolicy { get; set; } = BalancePolicy.RoundRobin;

            /// <summary>
            /// 失败超时时间
            /// </summary>
            public int FailTimeout { get; set; } = 1000 * 10;

            /// <summary>
            /// 失败超时时间内，允许发生的最大失败次数
            /// </summary>
            public int MaxFails { get; set; } = 1;

            /// <summary>
            /// 用此IP地址覆盖真实的IP地址（当PreferIpAddress=true时有效）
            /// </summary>
            public string IpAddress { get; set; } = "127.0.0.1";

            /// <summary>
            /// 使用IP地址代替HostName
            /// </summary>
            public bool PreferIpAddress { get; set; } = true;

            /// <summary>
            /// 当Http请求发生配置所述状况时，将选择另一个实例重试，默认值Error|Timeout
            /// </summary>
            public NextWhen NextWhen { get; set; } = NextWhen.Error | NextWhen.Timeout;

            /// <summary>
            /// 限制重试次数，默认值为0（表示不限制）
            /// </summary>
            public int NextTries { get; set; } = 0;

            /// <summary>
            /// 限制重试时间，默认值为0ms（表示不限制）
            /// </summary>
            public int NextTimeout { get; set; } = 0;
        }
    }
}
