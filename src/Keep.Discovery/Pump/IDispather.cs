using Keep.Discovery.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Keep.Discovery.Pump
{
    internal interface IDispatcher : IDisposable
    {
        void Pumping();

        Task<bool> AcceptThenDispatchAsync(HandlingContext context);
    }
}
