using System.Net;

namespace dotnet5_winservice_demo.Containers.WebApiContainers
{
    public interface IUserContainer
    {
        void LoginAccount(HttpListenerRequest request, HttpListenerResponse response);
        void GetCurrentUser(HttpListenerRequest request, HttpListenerResponse response);
    }
}
