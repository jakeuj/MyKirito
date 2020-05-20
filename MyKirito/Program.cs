using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyKirito
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var host = CreateHostBuilder(args).Build();

            var todoRepository = host.Services.GetRequiredService<IInitializationService>();
            await todoRepository.Initialization();

            await host.RunAsync();

            await host.StopAsync();

            return 0;
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<MyKiritoService>();
                    services.AddHostedService<LifetimeEventsHostedService>();
                    services.AddSingleton<IYourKiritoService, YourKiritoService>();
                    services.AddSingleton<IInitializationService, InitializationService>();
                });
    }
}