using dotnet5_winservice_demo.Utility;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
 
namespace dotnet5_winservice_demo.Containers.WebApiContainers
{
    public class UserContainer : IUserContainer
    {
        public void LoginAccount(HttpListenerRequest request, HttpListenerResponse response)
        {
            var strJson = HttpListenerHelper.GetRequestBodyJson(request);
            using (var doc = JsonDocument.Parse(strJson))
            {
                if (doc.RootElement.TryGetProperty("username", out var userNameElement) && doc.RootElement.TryGetProperty("password", out var passwordElement))
                {
                    var strUserName = userNameElement.GetString();
                    var strPassword = passwordElement.GetString();
                    //这里固定用户名为"admin"，密码为"password"，在加入数据库后可改成从数据库中读取比较
                    if (strUserName == "admin" && strPassword == "password")
                    {
                        var cookie = new Cookie("DotNet5WinServiceDemoUserName", strUserName);
                        cookie.Expires = DateTime.Today.AddDays(1);
                        cookie.Path = "/";
                        if (request.Url != null)
                        {
                            cookie.Domain = request.Url.Host;
                        }
                        response.AppendCookie(cookie);

                        using (var stream = new MemoryStream())
                        {
                            using (var writer = new Utf8JsonWriter(stream, GlobalVariables.JsonWriterOptions))
                            {
                                writer.WriteStartObject();
                                writer.WriteString("status", "ok");
                                writer.WriteString("type", "account");
                                writer.WriteString("currentAuthority", "admin");
                                writer.WriteEndObject();
                            }

                            strJson = Encoding.UTF8.GetString(stream.ToArray());
                            response.StatusCode = 200;
                            response.OutputStream.Write(Encoding.UTF8.GetBytes(strJson));
                            response.OutputStream.Close();
                            return;
                        }
                    }
                }
            }
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, GlobalVariables.JsonWriterOptions))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", "error");
                    writer.WriteString("type", "account");
                    writer.WriteString("currentAuthority", "guest");
                    writer.WriteEndObject();
                }

                strJson = Encoding.UTF8.GetString(stream.ToArray());
                response.StatusCode = 401;
                response.OutputStream.Write(Encoding.UTF8.GetBytes(strJson));
                response.OutputStream.Close();
            }
        }

        public void GetCurrentUser(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Cookies["DotNet5WinServiceDemoUserName"] == null || request.Cookies["DotNet5WinServiceDemoUserName"].Value != "admin")
            {
                response.StatusCode = 401;
                response.OutputStream.Close();
                return;
            }
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, GlobalVariables.JsonWriterOptions))
                {
                    writer.WriteStartObject();
                    writer.WriteString("name", "超级管理员");
                    writer.WriteString("avatar", "pro_icon.svg");
                    writer.WriteString("userid", "admin");
                    writer.WriteString("email", "admin@pro.ant.design");
                    writer.WriteString("signature", "超级管理员");
                    writer.WriteString("title", "超级管理员");
                    writer.WriteString("group", "超级管理员");
                    writer.WriteStartArray("tags");
                    writer.WriteStartObject();
                    writer.WriteString("key", "0");
                    writer.WriteString("label", "超级管理员");
                    writer.WriteEndObject();
                    writer.WriteEndArray();
                    writer.WriteNumber("notifyCount", 0);
                    writer.WriteNumber("unreadCount", 0);
                    writer.WriteString("country", "China");
                    writer.WriteStartObject("geographic");
                    writer.WriteStartObject("province");
                    writer.WriteString("label", "辽宁省");
                    writer.WriteString("key", "330000");
                    writer.WriteEndObject();
                    writer.WriteStartObject("city");
                    writer.WriteString("label", "沈阳市");
                    writer.WriteString("key", "330100");
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                    writer.WriteString("address", "浑南新区");
                    writer.WriteString("phone", "");
                    writer.WriteEndObject();
                }

                var strJson = Encoding.UTF8.GetString(stream.ToArray());
                response.StatusCode = 200;
                response.OutputStream.Write(Encoding.UTF8.GetBytes(strJson));
                response.OutputStream.Close();
            }
        }
    }
}