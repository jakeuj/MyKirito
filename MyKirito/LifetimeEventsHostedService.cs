using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyKirito
{
    internal class LifetimeEventsHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IYourKiritoService _yourService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IHostApplicationLifetime _appLifetime;

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger,
            IYourKiritoService yourService,
            IHostEnvironment hostEnvironment,
            IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _yourService = yourService;
            _hostEnvironment = hostEnvironment;
            _appLifetime = appLifetime;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync");
            _logger.LogDebug(Global.GameOptions.Token);
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            await DoWork(cancellationToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DoWork");            
            while (!cancellationToken.IsCancellationRequested)
            {
                // 啟動遊戲服務
                var output = await _yourService.StartLoop();
                // 定時執行
                await Task.Delay(output * 1000, cancellationToken);
            }
        }

        private async Task WriteJson(CancellationToken cancellationToken,string path, string name)
        {
            _logger.LogInformation("WriteJson");
            try
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, name)))
                {
                    await outputFile.WriteAsync(Global.GameOptions.ToJsonString(false));
                }
                Console.WriteLine($"寫入位於 {path} 的設定檔 {name} 完成");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"寫入位於 {path} 的設定檔 {name} 失敗");
                Console.WriteLine(path);
                Console.WriteLine(ex.Message);
            }            
            _logger.LogInformation("WriteJson End");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync");
            await WriteJson(cancellationToken, Global.JsonPath, Global.JsonFileName);
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");

            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}
