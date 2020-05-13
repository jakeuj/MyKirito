using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyKirito
{
    internal class Program
    {
        // 行動列舉值
        public enum ActionEnum
        {
            Sit,
            Good,
            Train,
            Eat,
            Girl,
            Hunt
        }
        
        public enum CharEnum
        {
            Kirito,
            Hatsumi,
            Eugeo,
            Sado,
            Klein,
            Kibaou,Hitsugaya,Muguruma,CharizardX,Reiner,Sinon,Lisbeth,Silica,Rosalia,Hinamori,Aizen
        }
        
        // 預設角色
        private static CharEnum _defaultChar = CharEnum.Kirito;
        
        // 行動版本號
        private const int DoActionVersion = 2;
        
        // 遊戲檢查時間闕值 (預設:90秒)
        private const int CheckTime = 100000;

        // 遊戲Pvp檢查時間闕值 (預設:402秒)
        private const int PvpTime = 402;
        
        // Token
        private static string _token = string.Empty;
        
        //總獲得屬性點
        private static int _totalPoints;

        // Pvp計時器
        private static DateTime _nextPvpTime = DateTime.Now.AddSeconds(PvpTime);

        // 預設動作
        private static ActionEnum _defaultAct = ActionEnum.Girl;

        // 額外屬性等級闕值
        private static readonly int[] AddPointLevel = {15, 20, 23, 25};

        // 程式進入點
        private static async Task<int> Main(string[] args)
        {
            if (args.Length > 0)
                _token = args[0];
            // 初始化
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
            Console.WriteLine("[Required] Input your token:");
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
                Console.WriteLine("[Required] Input your token:");
            }

            // 更新預設動作
            Console.Write("[Optional] Input your action from:");
            foreach (var name in Enum.GetNames(typeof(ActionEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
            {
                _defaultAct = (ActionEnum) Enum.Parse(typeof(ActionEnum), newInput);
            }
            Console.WriteLine($"Action is set to: {_defaultAct.ToString()}");
            
            // 更新預設角色
            Console.Write("[Optional] Input your char from:");
            foreach (var name in Enum.GetNames(typeof(CharEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput))
            {
                _defaultChar = (CharEnum) Enum.Parse(typeof(CharEnum), newInput);
            }
            Console.WriteLine($"Char is set to: {_defaultChar.ToString()}");
        }

        //檢查獲得屬性點數
        private static int CheckPoint(int input)
        {
            //超過最大等級時：超過幾級就拿幾點 + 所有獎勵等級各拿一點(有幾個限制就拿幾點)
            if (input > AddPointLevel.Max())
                return input - AddPointLevel.Max() + AddPointLevel.Count();
            //否則等級滿足幾個門檻就拿點幾額外屬性點
            return AddPointLevel.Count(x => input >= x);
        }

        // 角色資料傳輸物件
        public class MyKiritoDto
        {
            public string NickName { get; set; }
            public int Lv { get; set; }
            public int Exp { get; set; }
            public bool Dead { get; set; }
        }

        // 遊戲服務介面
        public interface IMyService
        {
            // 遊戲主畫面操作
            Task<string> DoAction(ActionEnum input);

            // 遊戲投胎機制
            Task<string> ReIncarnation(int freePoints);

            // 取得遊戲角色資料
            Task<MyKiritoDto> GetMyKiritoFn();
            Task<string> GetUserList(int exp);
        }

        // 遊戲服務介面實作類別
        public class MyService : IMyService
        {
            // 發起Http要求
            private readonly IHttpClientFactory _clientFactory;

            // DI注入
            public MyService(IHttpClientFactory clientFactory)
            {
                _clientFactory = clientFactory;
            }

            /// <summary>
            ///     角色轉生
            /// </summary>
            /// <param name="freePoints">升級點數</param>
            /// <returns>結果</returns>
            public async Task<string> ReIncarnation(int freePoints)
            {
                Console.WriteLine($"獲得 {freePoints} 屬性");
                var request = new HttpRequestMessage(HttpMethod.Post, "my-kirito/reincarnation");
                // 將點數點到生命值
                var restLoad =
                    $"{{\"character\":\"{_defaultChar.ToString().ToLower()}\",\"rattrs\":{{\"hp\":{freePoints},\"atk\":0,\"def\":0,\"stm\":0,\"agi\":0,\"spd\":0,\"tec\":0,\"int\":0,\"lck\":0}},\"useReset\":false}}";
                request.Content = new StringContent(restLoad, Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出轉生請求
                var response = await client.SendAsync(request);
                // 結果
                if (!response.IsSuccessStatusCode) return $"StatusCode: {response.StatusCode}";
                // 總計下屬性點
                _totalPoints += freePoints;
                return await response.Content.ReadAsStringAsync();
            }

            /// <summary>
            ///     角色行動
            /// </summary>
            /// <param name="input">要汁妹還是做啥來著</param>
            /// <returns>結果</returns>
            public async Task<string> DoAction(ActionEnum input)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "my-kirito/doaction")
                {
                    Content = new StringContent($"{{\"action\":\"{input.ToString().ToLower()}{DoActionVersion}\"}}", Encoding.UTF8,
                        "application/json")
                };
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                // 結果
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                return $"StatusCode: {response.StatusCode}";
            }

            /// <summary>
            ///     取得角色資料
            /// </summary>
            /// <returns>角色物件JSON</returns>
            public async Task<MyKiritoDto> GetMyKiritoFn()
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "getMyKiritoFn");
                var client = _clientFactory.CreateClient("kiritoInfo");
                var response = await client.SendAsync(request);
                // 不成功則空
                if (!response.IsSuccessStatusCode) return new MyKiritoDto();
                // 成功則反序列化後返回角色物件
                var res = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MyKiritoDto>(res,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            }

            // 查詢PvP玩家列表
            public async Task<string> GetUserList(int exp)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"user-list?exp={exp}");
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                // 結果
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    using (var document = JsonDocument.Parse(res))
                    {
                        var root = document.RootElement;
                        var studentsElement = root.GetProperty("userList");
                        foreach (var student in studentsElement.EnumerateArray())
                        {
                            if (!student.TryGetProperty("uid", out var gradeElement) ||
                                !student.TryGetProperty("lv", out var lvElement) ||
                                !student.TryGetProperty("color", out var colorElement) ||
                                colorElement.GetString() == "grey") continue;

                            var uid = gradeElement.GetString();

                            var lv = lvElement.GetInt32();

                            var (isSuccess, msg) = await Challenge(uid, lv);

                            if (isSuccess)
                            {
                                _nextPvpTime = _nextPvpTime.AddSeconds(PvpTime);
                                return msg;
                            }
                        }
                    }

                    return await response.Content.ReadAsStringAsync();
                }

                return $"StatusCode: {response.StatusCode}";
            }

            // 出戰Pvp
            private async Task<(bool isSuccess, string msg)> Challenge(string uid, int lv)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "challenge")
                {
                    Content = new StringContent($"{{\"type\":1,\"opponentUID\":\"{uid}\",\"shout\":\"\",\"lv\":{lv}}}",
                        Encoding.UTF8,
                        "application/json")
                };
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                // 結果
                return response.IsSuccessStatusCode
                    ? (true, await response.Content.ReadAsStringAsync())
                    : (false, $"StatusCode: {response.StatusCode}");
            }
        }

        // 背景工作排程服務
        internal interface IScopedProcessingService
        {
            Task DoWork(CancellationToken stoppingToken);
        }

        internal class ScopedProcessingService : IScopedProcessingService
        {
            private readonly ILogger _logger;

            // 排程主要使用遊戲主服務
            private readonly IMyService _myService;
            private int _executionCount;

            public ScopedProcessingService(ILogger<ScopedProcessingService> logger, IMyService myService)
            {
                _logger = logger;
                _myService = myService;
            }

            // 執行工作
            public async Task DoWork(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _executionCount++;

                    _logger.LogInformation(
                        "Scoped Processing Service is working. Count: {Count}", _executionCount);
                    // 取得角色資料
                    var myKirito = await _myService.GetMyKiritoFn();
                    // Log角色資料
                    Console.WriteLine(JsonSerializer.Serialize(myKirito,
                        new JsonSerializerOptions {WriteIndented = true}));
                    // 轉生限制條件：滿十等或死亡
                    if (myKirito.Dead || myKirito.Lv >= 10)
                    {
                        // 計算轉生屬性點數
                        var freePoints = CheckPoint(myKirito.Lv);
                        // 開始轉生
                        var result = await _myService.ReIncarnation(freePoints);
                        // Log轉生結果
                        Console.WriteLine(result);
                    }
                    else
                    {
                        // 日常動作：汁妹之類的
                        var result = await _myService.DoAction(_defaultAct);
                        // Log動作結果
                        Console.WriteLine(result);
                        // PVP 
                        if (DateTime.Now > _nextPvpTime)
                        {
                            result = await _myService.GetUserList(myKirito.Exp);
                            Console.WriteLine(result);
                        }
                        else
                        {
                            Console.WriteLine($"Next PVP time is {_nextPvpTime.ToShortTimeString()}");
                        }
                    }

                    Console.WriteLine($"獲得屬性小計：{_totalPoints}");
                    // 定時執行
                    await Task.Delay(CheckTime, stoppingToken);
                }
            }
        }

        // 背景服務
        public class ConsumeScopedServiceHostedService : BackgroundService
        {
            private readonly ILogger<ConsumeScopedServiceHostedService> _logger;

            public ConsumeScopedServiceHostedService(IServiceProvider services,
                ILogger<ConsumeScopedServiceHostedService> logger)
            {
                Services = services;
                _logger = logger;
            }

            public IServiceProvider Services { get; }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service running.");

                await DoWork(stoppingToken);
            }

            private async Task DoWork(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service is working.");

                using (var scope = Services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IScopedProcessingService>();

                    await scopedProcessingService.DoWork(stoppingToken);
                }
            }

            public override async Task StopAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service is stopping.");

                await Task.CompletedTask;
            }
        }
    }
}