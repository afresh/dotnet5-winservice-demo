using System.Collections.Generic;

namespace dotnet5_winservice_demo.Options
{
    public class HostedServiceOptions
    {
        public const string HostedService = "HostedService";

        public int WorkerInterval { get; set; }

        public List<string> HttpListenerPrefixes { get; set; } //对应 HTTP 协议侦听地址
    }
}
