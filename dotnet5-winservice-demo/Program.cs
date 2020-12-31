using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Xml;

namespace dotnet5_winservice_demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead("log4net.config")); //读取log4net配置文件
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy)); //创建log4net仓储
            log4net.Config.XmlConfigurator.Configure(repo, log4NetConfig["log4net"]); //配置log4net

            var host = CreateHostBuilder(args).Build(); //主机托管对象

            var logger = host.Services.GetRequiredService<ILogger<Program>>(); //获取 ILogger 实例
            logger.LogInformation("Host created."); //打印Information级别的日志

            host.Run(); //运行
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() //支持Windows服务
                .UseSystemd() //支持Linux守护进程
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // The ILoggingBuilder minimum level determines the
                    // the lowest possible level for logging. The log4net
                    // level then sets the level that we actually log at.
                    logging.AddLog4Net(); //添加log4net
                    logging.SetMinimumLevel(LogLevel.Debug); //设置最低级别为Debug
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>(); //每添加一个托管服务类，都需要在此处添加配置。
                });
            }
}
