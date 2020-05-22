using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyKirito
{
    public class MyKiritoService : IMyKiritoService
    {
        // _httpClient isn't exposed publicly
        private readonly HttpClient Client;
        private readonly ILogger _logger;
        public MyKiritoService(HttpClient client, ILogger<MyKiritoService> logger)
        {
            _logger = logger;
            _logger.LogDebug(Global.GameOptions.Token);
            client.BaseAddress = new Uri("https://mykirito.com/api/");
            client.DefaultRequestHeaders.Add("Accept",
                "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("accept-language",
                "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-CN;q=0.6");
            client.DefaultRequestHeaders.Add("origin",
                "https://mykirito.com");
            client.DefaultRequestHeaders.Add("User-Agent",
                "ozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            client.DefaultRequestHeaders.Add("token", Global.GameOptions.Token);
            Client = client;
        }        

        public async Task<ProfileDto> GetProfile(string uid)
        {
            var response = await Client.GetAsync("profile/"+uid);
            Console.WriteLine($"取得 {uid} 資料 {response.StatusCode}");
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsJsonAsync<ProfileDto>();
                Console.WriteLine($"角色：{output.profile.nickname}, 等級{output.profile.lv}, 狀態" + (output.profile.dead ? "死亡" : "正常"));
                return output;
            }
            await OnErrorOccur(response.StatusCode, content, "GetProfile");
            return null;
        }

        public async Task<MyKirito> GetMyKirito()
        {
            var response = await Client.GetAsync("my-kirito");
            Console.WriteLine("{0} {1}", "取得自己資料", response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsJsonAsync<MyKirito>();
                Console.WriteLine($"角色：{output.Nickname}, 等級{output.Lv}, 狀態" + (output.Dead ? "死亡" : "正常"));
                return output;
            }
            await OnErrorOccur(response.StatusCode, content, "GetMyKirito");
            return null;
        }

        public async Task<ActionOutput> DoAction(ActionEnum input)
        {
            var json = new ActionInput {Action = input.ToString().ToLower() + Global.DoActionVersion};
            HttpContent contentPost = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("my-kirito/doaction", contentPost);
            Console.WriteLine("{0} {1} {2}", "開始行動", input.GetDescriptionText(), response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsJsonAsync<ActionOutput>();                
                Console.WriteLine(output.Message);
                if (output != null && output.Gained != null && output.Gained.Hp != null)
                    await WriteJson(output.Gained, ActionEnum.None);
                return output;
            }

            await OnErrorOccur(response.StatusCode, content, "行動", 1);
            return null;
        }

        public async Task<string> ReIncarnation(long freePoints)
        {
            var json = new ReIncarnationInput
            {
                Character = Global.GameOptions.DefaultChar.ToString().ToLower(), UseReset = false,
                Rattrs = new Rattrs {Int = freePoints}
            };
            Console.WriteLine("準備轉生資料" + json.ToJsonString());
            HttpContent contentPost = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("my-kirito/reincarnation", contentPost);
            Console.WriteLine("以 {0} 點屬性 開始轉生 {1}", freePoints, response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsStringAsync();
                Console.WriteLine(output);
            }

            await OnErrorOccur(response.StatusCode, content, "ReIncarnation");
            return null;
        }

        public async Task<UserListDto> GetUserByName(string nickName)
        {
            var getRequestString = "search?nickname=" + nickName;
            Console.Write("取得 {0} 對手資料 ", nickName);
            return await GetUserList(getRequestString);
        }

        public async Task<UserListDto> GetUserListByLevel(long level, long page=1)
        {
            var getRequestString = $"user-list?lv={level}&page={page}";
            Console.Write($"取得 {level} 等級的對手清單資料{page} ");
            return await GetUserList(getRequestString);
        }

        private async Task<UserListDto> GetUserList(string getRequestString)
        {
            var response = await Client.GetAsync(getRequestString);
            Console.WriteLine(response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsJsonAsync<UserListDto>();
                Console.WriteLine($"成功取得 {output.UserList.Count} 筆對手資料");
                return output;
            }

            await OnErrorOccur(response.StatusCode, content, "GetUserList");
            return null;
        }

        public async Task<(BattleLog battleLog, HttpStatusCode statusCode, ErrorOutput errorOutput)> Challenge(long userLv, string userUid, string userNickName)
        {
            var json = new ChallengeInput
            { Lv = userLv, OpponentUid = userUid, Type = (int)Global.GameOptions.DefaultFight, Shout = string.Empty };
            HttpContent contentPost = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("challenge", contentPost);
            Console.WriteLine($"嘗試與[{userLv}][{userNickName}][{userUid}] 進行 {Global.GameOptions.DefaultFight.GetDescriptionText()} {response.StatusCode}");
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                var output = await content.ReadAsJsonAsync<BattleLog>();
                output.Messages.Select(x => x.M).ToList().ForEach(Console.WriteLine);
                Console.WriteLine($"與 [{userLv}] {userNickName} 戰鬥 {output.Result} 獲得 {output.Gained.Exp} 經驗");
                if (output != null && output.Gained != null && output.Gained.Hp != null)
                    await WriteJson(output.Gained, ActionEnum.None, output.Result == "勝利");
                
                return (output, response.StatusCode,null);
            }            
            return (null, response.StatusCode, await OnErrorOccur(response.StatusCode, content, "戰鬥", 2));
        }

        private async Task WriteJson(Gained gained, ActionEnum act, bool win=false)
        {
            _logger.LogDebug($"寫入 {Global.JsonPath} {Global.CsvFileName} 開始");
            Console.WriteLine("已升級，準備寫入升級資訊：");
            Console.WriteLine(gained.ToJsonString());
            try
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Global.JsonPath, Global.CsvFileName), true))
                {
                    await outputFile.WriteLineAsync($"{act},{win},{gained.Hp},{gained.Atk},{gained.Def},{gained.Stm},{gained.Agi},{gained.Spd},{gained.Tec},{gained.Int},{gained.Lck},{gained.PrevLv},{gained.NextLv},{gained.Exp},{gained.PrevTitle}");
                }
                Console.WriteLine("升級紀錄完成");              
                _logger.LogInformation($"寫入 {Global.JsonPath} {Global.CsvFileName} 成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("升級紀錄失敗");
                _logger.LogError($"寫入 {Global.JsonPath} {Global.CsvFileName} 失敗");
                Console.WriteLine(ex.Message);
            }
            _logger.LogDebug($"寫入 {Global.JsonPath} {Global.CsvFileName} 完成");
        }

        private async Task<ErrorOutput> OnErrorOccur(HttpStatusCode statusCode, HttpContent content, string message = "", int from = 0)
        {
            ErrorOutput errorOutput = null;
            if (statusCode == HttpStatusCode.Forbidden)
            {
                // 需驗證
                try
                {
                    errorOutput = await content.ReadAsJsonAsync<ErrorOutput>();
                    Console.WriteLine(errorOutput.Error);
                    if (from == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if (from == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.WriteLine($"[{Global.MyKiritoDto.Nickname}] 驗證 [{message}] 後 [{Global.GameOptions.DefaultAct.GetDescriptionText()}] 按任意鍵繼續");                    
                }
                catch
                {
                    var output = await content.ReadAsStringAsync();
                    Console.WriteLine(output);
                    if (from == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if(from == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.WriteLine("換IP後按任意鍵繼續");
                }
                Console.ResetColor();
                Console.ReadKey();
            }
            else if (statusCode == HttpStatusCode.BadRequest)
            {
                // 冷卻中or你的角色現在是死亡狀態
                try
                {
                    errorOutput = await content.ReadAsJsonAsync<ErrorOutput>();
                    Console.WriteLine(errorOutput.Error);
                }
                catch
                {
                    var output = await content.ReadAsStringAsync();
                    Console.WriteLine(output);
                }
            }
            else
            {
                // 未知錯誤
                try
                {
                    var output = await content.ReadAsStringAsync();
                    Console.WriteLine(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine("未知錯誤");
                    Console.WriteLine(e);
                }
            }
            return errorOutput;
        }
    }
}