using dotnet5_winservice_demo.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet5_winservice_demo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

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
    }
}
