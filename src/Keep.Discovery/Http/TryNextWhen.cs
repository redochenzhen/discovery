using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Http
{
    [Flags]
    internal enum TryNextWhen
    {
        Never = 0,

        Error = 0x0001,
        Timeout = 0x0002,
        InvalidHeader = 0x0004,

        Http500 = 0x0010,
        Http502 = 0x0020,
        Http503 = 0x0040,

        Http403 = 0x0100,
        Http404 = 0x0100,

        NonIdemponent = 0x1000,
    }
}
