using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 服务类型
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Restful服务
        /// </summary>
        Rest,
        
        /// <summary>
        /// gGRP服务
        /// </summary>
        Grpc,

        /// <summary>
        /// WCF服务
        /// </summary>
        Wcf
    }
}
