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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HostedServiceOptions = _configuration.GetSection(HostedServiceOptions.HostedService).Get<HostedServiceOptions>(); //�󶨲�����ָ��������

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(HostedServiceOptions.WorkerInterval, stoppingToken); //�����й�������ѯʱ��
            }
        }
    }
}
