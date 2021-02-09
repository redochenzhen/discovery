using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 服务状态
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// 在线
        /// </summary>
        Up,

        /// <summary>
        /// 下线
        /// </summary>
        Down,

        /// <summary>
        /// 备用
        /// </summary>
        Backup
    }
}
