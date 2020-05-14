using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyKirito
{
    internal class Program
    {
        // Token
        private static string _token = string.Empty;

        // 詢問設定開關
        private static bool _isAsk= true;

        // 程式進入點
        private static async Task<int> Main(string[] args)
        {
            // 接收Token參數
            if (args.Length > 0)
                _token = args[0];
            // 接收重生等級參數
            if (args.Length > 1 && int.TryParse(args[1], out var newReLevel))
                AppSettings._defaultReIncarnationLevel = newReLevel;
            // 接收行為參數
            if (args.Length > 2 && Enum.TryParse(args[2], true, out ActionEnum newActEnum))
                AppSettings._defaultAct = newActEnum;
            // 接收角色參數
            if (args.Length > 3 && Enum.TryParse(args[3], true, out CharEnum newCharEnum))
                AppSettings._defaultChar = newCharEnum;
            // 接收戰鬥參數
            if (args.Length > 4 && Enum.TryParse(args[4], true, out FightEnum newFightEnum))
                AppSettings._defaultFight = newFightEnum;
            // 接收亂數最大值參數
            if (args.Length > 5 && int.TryParse(args[5], out var newRandTime))
                AppSettings._randTime = newRandTime;
            // 接收安靜模式參數
            if (args.Length > 6 && int.TryParse(args[6], out var isSilence) && isSilence > 0)
                _isAsk = false;

            // 初始化
            if(_isAsk)
                Init();
            using var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                        .AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 遊戲API端點
                    services.AddHttpClient("kiritoAPI", c =>
                    {
                        c.BaseAddress = new Uri("https://mykirito.com/api/");
                        c.DefaultRequestHeaders.Add("token", _token);
                    });
                    // 遊戲公開資訊端點
                    services.AddHttpClient("kiritoInfo", c =>
                    {
                        c.BaseAddress = new Uri("https://us-central1-kirito-1585904519813.cloudfunctions.net/");
                        c.DefaultRequestHeaders.Add("token", _token);
                    });
                    // 註冊遊戲服務
                    services.AddTransient<IMyService, MyService>();
                    // 註冊排程服務
                    services.AddHostedService<ConsumeScopedServiceHostedService>();
                    services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
                })
                .Build();
            // 開始排程服務
            await host.StartAsync();

            // 等待服務關閉
            await host.WaitForShutdownAsync();

            return 0;
        }

        private static void Init()
        {
            // 更新授權Token
            Console.WriteLine("[必要] 輸入 Token:");
            string newInput;
            while (true)
            {
                newInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newInput)) _token = newInput;
                if (!string.IsNullOrWhiteSpace(_token))
                {
                    Console.WriteLine($"Token is set to: {_token}");
                    break;
                }

                Console.WriteLine("[Required] 輸入 token:");
            }

            // 更新預設角色
            Console.WriteLine($"[選填] 設定自動重生等級,0=不自殺(預設：{AppSettings._defaultReIncarnationLevel}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newLevel))
                AppSettings._defaultReIncarnationLevel = newLevel;
            Console.WriteLine($"ReIncarnation level is set to: {AppSettings._defaultReIncarnationLevel}");

            // 更新預設動作
            Console.Write($"[選填] 設定主要動作(預設：{AppSettings._defaultAct}):");
            foreach (var name in Enum.GetNames(typeof(ActionEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out ActionEnum newActEnum))
                AppSettings._defaultAct = newActEnum;
            Console.WriteLine($"Action is set to: {AppSettings._defaultAct}");

            // 更新預設角色
            Console.Write($"[選填] 設定重生角色(預設：{AppSettings._defaultChar}):");
            foreach (var name in Enum.GetNames(typeof(CharEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out CharEnum newCharEnum))
                AppSettings._defaultChar = newCharEnum;
            Console.WriteLine($"Char is set to: {AppSettings._defaultChar}");

            // 更新自動戰鬥
            Console.Write($"[選填] 設定戰鬥模式(預設：{AppSettings._defaultFight}):");
            foreach (var name in Enum.GetNames(typeof(FightEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out FightEnum newFightEnum))
                AppSettings._defaultFight = newFightEnum;
            Console.WriteLine($"Fight is set to: {AppSettings._defaultFight}");

            // 更新浮動CD時間
            Console.WriteLine($"[選填] 設定基礎冷卻時間額外增加之浮動CD秒數上限(預設：{AppSettings._randTime}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newRandTime))
                AppSettings._randTime = newRandTime;
            Console.WriteLine($"RandTime is set to: {AppSettings._randTime}");
        }
    }
}