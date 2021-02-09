using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Worker
{
    /// <summary>
    /// 后台任务节后
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// 启动
        /// </summary>
        void Start();

        /// <summary>
        /// 死亡脉冲
        /// </summary>
        void Pulse();

        /// <summary>
        /// 重启
        /// </summary>
        /// <param name="force"></param>
        void Restart(bool force = false);

        /// <summary>
        /// 终止
        /// </summary>
        void Stop();
    }
}
