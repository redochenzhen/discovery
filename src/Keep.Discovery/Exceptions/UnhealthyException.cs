using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Exceptions
{
    public class UnhealthyException : Exception
    {
        public UnhealthyException(string message) : base(message) { }

        public UnhealthyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
