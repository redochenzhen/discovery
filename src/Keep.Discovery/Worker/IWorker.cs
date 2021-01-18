using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Worker
{
    public interface IWorker
    {
        void Start();
        void Pulse();
        void Restart(bool force = false);
        void Stop();
    }
}
