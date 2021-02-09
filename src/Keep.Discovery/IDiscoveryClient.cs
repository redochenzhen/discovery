using Keep.Discovery.Contract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery
{
    /// <summary>
    /// 服务发现Client接口
    /// </summary>
    public interface IDiscoveryClient
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <returns></returns>
        Task RegisterAsync();

        /// <summary>
        /// 发现服务
        /// </summary>
        /// <returns></returns>
        Task DiscoverAsync();
    }
}
