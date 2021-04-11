using System;
using System.Collections.Generic;
using System.Text;
using Keep;

namespace Keep.Discovery.Eureka
{
    public class EurekaOptions
    {
        public const string EUREKA_CFG_PREFIX = "eureka";
        public const string EUREKA_CLIENT_CFG_PREFIX = "eureka:client";
        public const string EUREKA_INSTANCE_CFG_PREFIX = "eureka:instance";

        public string ServiceUrl { get; set; }

        public EurekaClientOptions Client { get; set; }

        public EurekaInstanceOptions Instance { get; set; }

        public class EurekaClientOptions
        { 
        }

        public class EurekaInstanceOptions
        {
            public string AppName { get; set; }
            public int Port { get; set; }
            public bool PreferIpAddress { get; set; }
        }
    }
}
