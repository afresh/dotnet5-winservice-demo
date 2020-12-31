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
            log4NetConfig.Load(File.OpenRead("log4net.config")); //��ȡlog4net�����ļ�
            var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy)); //����log4net�ִ�
            log4net.Config.XmlConfigurator.Configure(repo, log4NetConfig["log4net"]); //����log4net

            var host = CreateHostBuilder(args).Build(); //�����йܶ���

            var logger = host.Services.GetRequiredService<ILogger<Program>>(); //��ȡ ILogger ʵ��
            logger.LogInformation("Host created."); //��ӡInformation�������־

            host.Run(); //����
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() //֧��Windows����
                .UseSystemd() //֧��Linux�ػ�����
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // The ILoggingBuilder minimum level determines the
                    // the lowest possible level for logging. The log4net
                    // level then sets the level that we actually log at.
                    logging.AddLog4Net(); //���log4net
                    logging.SetMinimumLevel(LogLevel.Debug); //������ͼ���ΪDebug
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>(); //ÿ���һ���йܷ����࣬����Ҫ�ڴ˴�������á�
                });
            }
}
