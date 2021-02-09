using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Keep.Discovery
{
    /// <summary>
    /// 服务发现入口
    /// </summary>
    public interface IDiscoveryEntry
    {
        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="stoppingToken">停止令牌</param>
        void Boot(CancellationToken stoppingToken);
    }
}
