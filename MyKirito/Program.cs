using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyKirito
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Init.Initialization(args);
            return await Main1(args);
        }

        private static async Task<int> Main1(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("MyKirito.Program", LogLevel.Warning)
                        //.AddConsole()
                        ;
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<MyKiritoService>();
                    services.AddSingleton<IYourKiritoService, YourKiritoService>();
                    // 註冊排程服務
                    services.AddHostedService<ConsumeScopedServiceHostedService>();
                    services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
                }).UseConsoleLifetime();
            var host = builder.Build();
            // 開始排程服務
            await host.StartAsync();
            // 等待服務關閉
            await host.WaitForShutdownAsync();
            return 0;
        }
    }
}