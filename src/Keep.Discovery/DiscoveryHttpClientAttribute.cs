using System;
using System.Collections.Generic;
using System.Text;

namespace Keep.Discovery
{
    /// <summary>
    /// 标记向某个类/接口注入HttpClient对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscoveryHttpClientAttribute : Attribute
    {
        //public string BaseAddress { get; set; }

        public DiscoveryHttpClientAttribute() { }

        //public DiscoveryHttpClientAttribute(string baseAddress)
        //{
        //    BaseAddress = baseAddress;
        //}
    }
}
