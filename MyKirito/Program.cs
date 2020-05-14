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
            Kibaou,
            Hitsugaya,
            Muguruma,
            CharizardX,
            Reiner,
            Sinon,
            Lisbeth,
            Silica,
            Rosalia,
            Hinamori,
            Aizen,
            Asuna,
            kibaou,
            Hina,
            Agil,
            Rabbit,
            Kuradeel,
            Bradley
        }

        // 戰鬥列舉值
        public enum FightEnum
        {
            None = -1,
            Friend,
            Hard,
            Duo,
            Kill
        }

        // 行動版本號
        private const int DoActionVersion = 2;

        // 遊戲Pvp檢查時間闕值 (預設:400秒)
        private const int PvpTime = 400;

        // 遊戲檢查基本冷卻時間 (預設:100秒)
        private const int CheckTime = 100;

        // 亂數產生器
        private static readonly Random RandomCd = new Random();

        // 遊戲檢查額外冷卻時間浮動上限 (預設:100秒)
        private static int _randTime = 100;

        // 預設角色
        private static CharEnum _defaultChar = CharEnum.Kirito;

        // Token
        private static string _token = string.Empty;

        //總獲得屬性點
        private static int _totalPoints;

        //自動投胎等級 (0為不自動投胎，預設:0級)
        private static int _defaultReIncarnationLevel;

        // Pvp計時器
        private static DateTime _nextPvpTime = DateTime.Now.AddSeconds(PvpTime);

        // 預設動作
        private static ActionEnum _defaultAct = ActionEnum.Girl;

        // 預設Pvp
        private static FightEnum _defaultFight = FightEnum.None;


        // 額外屬性等級闕值
        private static readonly int[] AddPointLevel = {15, 20, 23, 25};

        private static bool _isAsk= true;

        // 程式進入點
        private static async Task<int> Main(string[] args)
        {
            // 接收Token參數
            if (args.Length > 0)
                _token = args[0];
            // 接收重生等級參數
            if (args.Length > 1 && int.TryParse(args[1], out var newReLevel))
                _defaultReIncarnationLevel = newReLevel;
            // 接收行為參數
            if (args.Length > 2 && Enum.TryParse(args[2], true, out ActionEnum newActEnum))
                _defaultAct = newActEnum;
            // 接收角色參數
            if (args.Length > 3 && Enum.TryParse(args[3], true, out CharEnum newCharEnum))
                _defaultChar = newCharEnum;
            // 接收戰鬥參數
            if (args.Length > 4 && Enum.TryParse(args[4], true, out FightEnum newFightEnum))
                _defaultFight = newFightEnum;
            // 接收亂數最大值參數
            if (args.Length > 5 && int.TryParse(args[5], out var newRandTime))
                _randTime = newRandTime;
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
            Console.WriteLine($"[選填] 設定自動重生等級,0=不自殺(預設：{_defaultReIncarnationLevel}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newLevel))
                _defaultReIncarnationLevel = newLevel;
            Console.WriteLine($"ReIncarnation level is set to: {_defaultReIncarnationLevel}");

            // 更新預設動作
            Console.Write($"[選填] 設定主要動作(預設：{_defaultAct}):");
            foreach (var name in Enum.GetNames(typeof(ActionEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out ActionEnum newActEnum))
                _defaultAct = newActEnum;
            Console.WriteLine($"Action is set to: {_defaultAct}");

            // 更新預設角色
            Console.Write($"[選填] 設定重生角色(預設：{_defaultChar}):");
            foreach (var name in Enum.GetNames(typeof(CharEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out CharEnum newCharEnum))
                _defaultChar = newCharEnum;
            Console.WriteLine($"Char is set to: {_defaultChar}");

            // 更新自動戰鬥
            Console.Write($"[選填] 設定戰鬥模式(預設：{_defaultFight}):");
            foreach (var name in Enum.GetNames(typeof(FightEnum))) Console.Write($" {name} ");
            Console.WriteLine();
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && Enum.TryParse(newInput, true, out FightEnum newFightEnum))
                _defaultFight = newFightEnum;
            Console.WriteLine($"Fight is set to: {_defaultFight}");

            // 更新浮動CD時間
            Console.WriteLine($"[選填] 設定基礎冷卻時間額外增加之浮動CD秒數上限(預設：{_randTime}):");
            newInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInput) && int.TryParse(newInput, out var newRandTime))
                _randTime = newRandTime;
            Console.WriteLine($"RandTime is set to: {_randTime}");
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
            Task<bool> DoAction(ActionEnum input);

            // 遊戲投胎機制
            Task<string> ReIncarnation(int freePoints);

            // 取得遊戲角色資料
            Task<MyKiritoDto> GetMyKiritoFn();
            Task<bool> GetUserList(int exp);
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
                    $"{{\"character\":\"{_defaultChar.ToString().ToLower()}\",\"rattrs\":{{\"hp\":0,\"atk\":0,\"def\":0,\"stm\":0,\"agi\":0,\"spd\":0,\"tec\":0,\"int\":{freePoints},\"lck\":0}},\"useReset\":false}}";
                request.Content = new StringContent(restLoad, Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出轉生請求
                var response = await client.SendAsync(request);
                // 結果
                if (!response.IsSuccessStatusCode) return $"StatusCode: {response.StatusCode}";
                // 總計下屬性點
                _totalPoints += freePoints;
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                return result;
            }

            /// <summary>
            ///     角色行動
            /// </summary>
            /// <param name="input">要汁妹還是做啥來著</param>
            /// <returns>結果</returns>
            public async Task<bool> DoAction(ActionEnum input)
            {
                Console.WriteLine($"DoAction {input} Result:");
                var request = new HttpRequestMessage(HttpMethod.Post, "my-kirito/doaction")
                {
                    Content = new StringContent($"{{\"action\":\"{input.ToString().ToLower()}{DoActionVersion}\"}}",
                        Encoding.UTF8,
                        "application/json")
                };
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                // 結果
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                return response.IsSuccessStatusCode;
            }

            /// <summary>
            ///     取得角色資料
            /// </summary>
            /// <returns>角色物件JSON</returns>
            public async Task<MyKiritoDto> GetMyKiritoFn()
            {
                Console.WriteLine("GetMyKiritoFn");
                var request = new HttpRequestMessage(HttpMethod.Get, "getMyKiritoFn");
                var client = _clientFactory.CreateClient("kiritoInfo");
                var response = await client.SendAsync(request);
                // 不成功則空
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("GetMyKiritoFn fail...");
                    return null;
                }
                // 成功則反序列化後返回角色物件
                var res = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MyKiritoDto>(res,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                // Log角色資料
                Console.WriteLine(JsonSerializer.Serialize(result,
                    new JsonSerializerOptions { WriteIndented = true }));
                return result;
            }

            // 查詢PvP玩家列表
            public async Task<bool> GetUserList(int exp)
            {
                bool isDone = false;
                var request = new HttpRequestMessage(HttpMethod.Get, $"user-list?exp={exp}");
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                // 結果
                if (response.IsSuccessStatusCode)
                {
                    using (var document = JsonDocument.Parse(result))
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

                            if (await Challenge(uid, lv))
                            {
                                _nextPvpTime = _nextPvpTime.AddSeconds(PvpTime);
                                isDone = true;
                                break;
                            }
                            await Task.Delay(2 * 1000);
                        }
                    }
                }
                else
                    Console.WriteLine(result);                
                return isDone;
            }

            // 出戰Pvp
            private async Task<bool> Challenge(string uid, int lv)
            {
                Console.WriteLine($"Challenge {uid} {lv}");
                var request = new HttpRequestMessage(HttpMethod.Post, "challenge")
                {
                    Content = new StringContent(
                        $"{{\"type\":{(int) _defaultFight},\"opponentUID\":\"{uid}\",\"shout\":\"\",\"lv\":{lv}}}",
                        Encoding.UTF8,
                        "application/json")
                };
                var client = _clientFactory.CreateClient("kiritoAPI");
                // 送出行動請求
                var response = await client.SendAsync(request);
                var result = response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                // 結果
                return response.IsSuccessStatusCode;
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
                    if (myKirito != null)
                    {
                        // 轉生限制條件：滿十等或死亡
                        if (myKirito.Dead || (_defaultReIncarnationLevel > 0 && myKirito.Lv >= _defaultReIncarnationLevel))
                        {
                            // 計算轉生屬性點數
                            var freePoints = CheckPoint(myKirito.Lv);
                            // 開始轉生
                            var result = await _myService.ReIncarnation(freePoints);
                        }
                        else
                        {
                            // 日常動作：汁妹之類的
                            await _myService.DoAction(_defaultAct);
                            // PVP 
                            if (_defaultFight != FightEnum.None && DateTime.Now > _nextPvpTime)
                            {
                                await _myService.GetUserList(myKirito.Exp);
                            }
                        }
                    }
                    // 定時執行
                    int addTime;
                    if (_randTime > 0)
                        addTime = (CheckTime + RandomCd.Next(1, _randTime));
                    else
                        addTime = CheckTime;
                    Console.WriteLine($"此次運行總獲得屬性點：{_totalPoints}, 下次戰鬥： {_nextPvpTime}, 等待 {addTime} 秒...");
                    await Task.Delay(addTime * 1000, stoppingToken);
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