using System.Collections.Generic;
using System.Net;
using System.Text;

namespace dotnet5_winservice_demo.Utility
{
    public class HttpListenerHelper
    {
        public static string GetRequestBodyJson(HttpListenerRequest request)
        {
            var bytes = new List<byte>();
            var buffer = new byte[2048];
            int readLen = -1;
            int len = 0;
            while (readLen != 0)
            {
                readLen = request.InputStream.Read(buffer, 0, buffer.Length);
                len += readLen;
                bytes.AddRange(buffer);
            }
            var requestBody = Encoding.UTF8.GetString(bytes.ToArray(), 0, len);
            return requestBody;
        }
    }
}
