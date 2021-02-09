using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 尝试另一个上游服务端的条件
    /// </summary>
    [Flags]
    public enum NextWhen
    {
        None = 0,
        /// <summary>
        /// 从不，禁用重试
        /// </summary>
        Never = 1,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 0x0002,

        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 0x0004,

        /// <summary>
        /// Header异常
        /// </summary>
        InvalidHeader = 0x0008,

        /// <summary>
        /// 500状态
        /// </summary>
        Http500 = 0x0010,

        /// <summary>
        /// 502状态
        /// </summary>
        Http502 = 0x0020,

        /// <summary>
        /// 503状态
        /// </summary>
        Http503 = 0x0040,

        /// <summary>
        /// 403状态
        /// </summary>
        Http403 = 0x0100,

        /// <summary>
        /// 404状态
        /// </summary>
        Http404 = 0x0100,

        /// <summary>
        /// 允许非幂等重试
        /// </summary>
        NonIdemponent = 0x1000,

        /// <summary>
        /// 只允许GET重试
        /// </summary>
        GetOnly = 0x200,
    }
}
