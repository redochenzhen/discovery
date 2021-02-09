using System;

namespace Keep.Discovery.Contract
{
    /// <summary>
    /// 负载均衡策略
    /// </summary>
    public enum BalancePolicy
    {
        /// <summary>
        /// 轮训
        /// </summary>
        RoundRobin,

        /// <summary>
        /// 随机
        /// </summary>
        Random,

        /// <summary>
        /// 二次随机选择
        /// </summary>
        RandomTwo
    }
}
