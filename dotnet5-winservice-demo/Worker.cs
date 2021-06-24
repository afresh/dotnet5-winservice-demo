using dotnet5_winservice_demo.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet5_winservice_demo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        private static HttpListener _httpListener; //����һ����̬�ļ�����

        public HostedServiceOptions HostedServiceOptions { get; private set; }

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration; //����ע��IConfiguration
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);

            HostedServiceOptions = _configuration.GetSection(HostedServiceOptions.HostedService).Get<HostedServiceOptions>(); //�󶨲�����ָ��������

            InitializeHttpListener(); //��ʼ�� HTTP Э��������

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                //�������ֹͣ����ô�����IsCancellationRequested�᷵��true�����Ǿ�Ӧ�ý���ѭ��
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(HostedServiceOptions.WorkerInterval, stoppingToken); //�����й�������ѯʱ��
                }
            }, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);

            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// ��ʼ�� HTTP Э��������
        /// </summary>
        private void InitializeHttpListener()
        {
            try
            {
                if (!HttpListener.IsSupported)
                {
                    _logger.LogWarning("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                    return;
                }

                if (HostedServiceOptions.HttpListenerPrefixes == null || HostedServiceOptions.HttpListenerPrefixes.Count == 0)
                {
                    throw new ArgumentException("prefixes");
                }

                if (_httpListener == null)
                {
                    _httpListener = new HttpListener();
                }

                foreach (var prefix in HostedServiceOptions.HttpListenerPrefixes)
                {
                    if (!_httpListener.Prefixes.Contains(prefix))
                    {
                        _httpListener.Prefixes.Add(prefix);
                    }
                }

                foreach (var httpListenerPrefix in _httpListener.Prefixes)
                {
                    if (!HostedServiceOptions.HttpListenerPrefixes.Contains(httpListenerPrefix))
                    {
                        _httpListener.Prefixes.Remove(httpListenerPrefix);
                    }
                }

                if (!_httpListener.IsListening)
                {
                    _httpListener.Start();
                }

                _httpListener.BeginGetContext(GetHttpListenerContext, null);

                _logger.LogDebug("Worker HttpListener Is Listening.");

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Worker HttpListener catching.");
            }
            finally
            {
                _logger.LogDebug("Worker HttpListener finally.");
            }
        }

        /// <summary>
        /// ��ȡ HTTP Э��������������
        /// </summary>
        /// <param name="ar"></param>
        private void GetHttpListenerContext(IAsyncResult ar)
        {
            //�����첽����
            _httpListener.BeginGetContext(GetHttpListenerContext, null);
            var id = Guid.NewGuid().ToString("N");
            _logger.LogDebug($"HttpListener recieve a request - {id}.");
            //���context����
            var context = _httpListener.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            if (request.Url != null)
            {
                _logger.LogDebug($"HttpListener request url - {request.Url.AbsoluteUri}");
            }
            ////�����js��ajax���󣬻��������ÿ����ip��ַ�����
            //context.Response.AppendHeader("Access-Control-Allow-Origin", "*"); //��̨��������ͨ������Ϊ�����ļ�
            //context.Response.AppendHeader("Access-Control-Allow-Headers", "ID,PW"); //��̨����������ã�ͨ������Ϊ�����ļ�
            //context.Response.AppendHeader("Access-Control-Allow-Method", "post"); //��̨�����������ã�ͨ������Ϊ�����ļ�
            context.Response.ContentType = "text/plain;charset=UTF-8"; //���ش��ı���ʽ������ΪUTF-8
            context.Response.AddHeader("Content-type", "text/plain"); //�����Ӧͷ��Ϣ
            context.Response.ContentEncoding = Encoding.UTF8;
            if (request.HttpMethod == "GET")
            {
                //����ͻ��˷��͵�GET���󲢷��ش�����Ϣ
                HttpListenerGetRequest(request, response);
            }
            else if (request.HttpMethod == "POST")
            {
                //����ͻ��˷��͵�POST���󲢷��ش�����Ϣ
                HttpListenerPostRequest(request, response);
            }

            _logger.LogDebug($"HttpListener return a response - {id}.");
        }

        /// <summary>
        /// GET����
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void HttpListenerGetRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (request.Url != null && !string.IsNullOrWhiteSpace(request.Url.AbsolutePath))
                {
                    //ע���ַ��ƥ��ȫ��ת������Сд
                    if (request.Url.AbsolutePath.ToLower().StartsWith("/logs"))
                    {
                        //TODO: output logs
                    }
                    else if (request.Url.AbsolutePath.ToLower().StartsWith("/api"))
                    {
                        switch (request.Url.AbsolutePath.ToLower())
                        {
                            default:
                                response.StatusCode = 404;
                                response.OutputStream.Close();
                                return;
                            case "/api/gethelloworld":
                                response.StatusCode = 200;
                                response.OutputStream.Write(Encoding.UTF8.GetBytes("Hello world!"));
                                response.OutputStream.Close();
                                break;
                        }
                        return;
                    }
                    else
                    {
                        //�ļ���Դ����
                        var filePath = request.Url.AbsolutePath.ToLower();
                        filePath = "dist" + filePath;
                        filePath = !request.Url.AbsolutePath.Contains(".") ? filePath + "/index.html" : filePath;
                        filePath = AppDomain.CurrentDomain.BaseDirectory + filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
                        if (File.Exists(filePath))
                        {
                            response.StatusCode = 200;
                            var extension = Path.GetExtension(filePath);
                            switch (extension)
                            {
                                default:
                                    response.ContentType = "";
                                    break;
                                case ".html":
                                case ".htm":
                                    response.ContentType = "text/html;charset=utf-8";
                                    break;
                                case ".js":
                                    response.ContentType = "application/javascript;charset=utf-8";
                                    break;
                                case ".css":
                                    response.ContentType = "text/css;charset=utf-8";
                                    break;
                                case ".png":
                                    response.ContentType = "image/png";
                                    break;
                                case ".jpg":
                                    response.ContentType = "image/jpeg";
                                    break;
                                case ".gif":
                                    response.ContentType = "image/gif";
                                    break;
                                case ".svg":
                                    response.ContentType = "image/svg+xml";
                                    break;
                                case ".swf":
                                    response.ContentType = "application/x-shockwave-flash";
                                    break;
                            }
                            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            var fileStreamLength = (int)fileStream.Length;
                            var buffer = new byte[fileStreamLength];
                            fileStream.Read(buffer, 0, fileStreamLength);
                            fileStream.Close();
                            fileStream.Dispose();
                            response.ContentLength64 = fileStreamLength;
                            response.OutputStream.Write(buffer, 0, fileStreamLength);
                            response.OutputStream.Close();
                            return;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                response.StatusCode = 500;
                _logger.LogError(exception, "HttpListener response error.");
                response.OutputStream.Write(Encoding.UTF8.GetBytes("Internal Server Error."));
                response.OutputStream.Close();
                return;
            }
            response.StatusCode = 404;
            response.OutputStream.Close();
        }

        /// <summary>
        /// POST����
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private void HttpListenerPostRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (request.Url != null && !string.IsNullOrWhiteSpace(request.Url.AbsolutePath))
                {
                    //ע���ַ��ƥ��ȫ��ת������Сд
                    if (request.Url.AbsolutePath.ToLower().StartsWith("/api"))
                    {
                        switch (request.Url.AbsolutePath.ToLower())
                        {
                            default:
                                response.StatusCode = 404;
                                response.OutputStream.Close();
                                return;
                        }
                        return;
                    }
                }
                response.StatusCode = 404;
                response.OutputStream.Close();
            }
            catch (Exception exception)
            {
                response.StatusCode = 500;
                _logger.LogError(exception, "HttpListener response error.");
                response.OutputStream.Write(Encoding.UTF8.GetBytes("Internal Server Error."));
                response.OutputStream.Close();
            }
        }
    }
}
