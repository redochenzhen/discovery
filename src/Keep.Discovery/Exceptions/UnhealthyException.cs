using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Exceptions
{
    /// <summary>
    /// 服务发现非健康异常
    /// </summary>
    public class UnhealthyException : Exception
    {
        /// <summary>
        /// UnhealthyException构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        public UnhealthyException(string message) : base(message) { }

        /// <summary>
        /// UnhealthyException构造函数
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public UnhealthyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
