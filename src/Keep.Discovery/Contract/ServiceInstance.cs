using Keep.Discovery.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery.Contract
{
    public class ServiceInstance : IServiceInstance
    {
        public string ServiceName { get; set; }

        public string HostName { get; private set; }

        public int Port { get; private set; }

        public bool IsSecure { get; private set; }

        public Uri Uri
        {
            get { return new Uri($"{(IsSecure ? "https" : "http")}://{HostName}:{Port}"); }
        }

        public IDictionary MetaData { get; set; }

        public ServiceState ServiceState { get; set; }

        public ServiceType ServiceType { get; set; } = ServiceType.Rest;

        public int Weight { get; set; }

        public ServiceInstance(string hostName, int port, bool isSecure = false)
        {
            HostName = hostName;
            Port = port;
            IsSecure = isSecure;
        }
    }
}
