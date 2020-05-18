﻿using System;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MyKirito
{
    public class MyKiritoService : IMyKiritoService
    {
        public MyKiritoService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://mykirito.com/api/");
            client.DefaultRequestHeaders.Add("Accept",
                "application/json, text/plain, */*");
            //client.DefaultRequestHeaders.Add("accept-encoding",
            //    "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("accept-language",
                "zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-CN;q=0.6");
            client.DefaultRequestHeaders.Add("origin",
                "https://mykirito.com");
            client.DefaultRequestHeaders.Add("User-Agent",
                "ozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            client.DefaultRequestHeaders.Add("token", AppSettings.Token);
            Client = client;
        }

        public HttpClient Client { get; }

        public async Task<MyKirito> GetMyKirito()
        {
            var response = await Client.GetAsync("my-kirito");
            Console.WriteLine("{0} {1}", "取得資料", response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                //await using var responseStream = await content.ReadAsStreamAsync();
                //await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);
                //var output = await decompressed.ReadAsJsonAsync<GZipStream, MyKirito>();
                var output = await content.ReadAsJsonAsync<MyKirito>();
                Console.WriteLine($"角色：{output.Nickname}, 等級{output.Lv}, 狀態" + (output.Dead ? "死亡" : "正常"));
                return output;
            }
            await OnErrorOccur(response.StatusCode, content, "GetMyKirito");
            return null;
        }

        public async Task<ActionOutput> DoAction(ActionEnum input)
        {
            var json = new ActionInput {Action = input.ToString().ToLower() + Const.DoActionVersion};
            HttpContent contentPost = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("my-kirito/doaction", contentPost);
            Console.WriteLine("{0} {1} {2}", "開始行動", input.GetDescriptionText(), response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                //await using var responseStream = await content.ReadAsStreamAsync();
                //await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);
                //var output = await decompressed.ReadAsJsonAsync<GZipStream, ActionOutput>();
                var output = await content.ReadAsJsonAsync<ActionOutput>();
                Console.WriteLine(output.Message);
                return output;
            }

            await OnErrorOccur(response.StatusCode, content, "DoAction");
            return null;
        }

        public async Task<string> ReIncarnation(long freePoints)
        {
            var json = new ReIncarnationInput
            {
                Character = AppSettings.DefaultChar.ToString().ToLower(), UseReset = false,
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

        public async Task<UserListDto> GetUserListByLevel(long level)
        {
            var getRequestString = "user-list?page=2&lv=" + level;
            Console.Write("取得 {0} 等級的對手清單資料 ", level);
            return await GetUserList(getRequestString);
        }        

        private async Task<UserListDto> GetUserList(string getRequestString)
        {
            var response = await Client.GetAsync(getRequestString);
            Console.WriteLine(response.StatusCode);
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                //await using var responseStream = await content.ReadAsStreamAsync();
                //await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);
                //var output = await decompressed.ReadAsJsonAsync<GZipStream, UserListDto>();
                var output = await content.ReadAsJsonAsync<UserListDto>();
                Console.WriteLine($"成功取得 {output.UserList.Count} 筆對手資料");
                return output;
            }

            await OnErrorOccur(response.StatusCode, content, "GetUserList");
            return null;
        }

        public async Task<BattleLog> Challenge(UserList user)
        {
            var json = new ChallengeInput
            { Lv = user.Lv, OpponentUid = user.Uid, Type = (int)AppSettings.DefaultFight, Shout = string.Empty };
            HttpContent contentPost = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("challenge", contentPost);
            Console.WriteLine($"嘗試與等級 {user.Lv} 的玩家 {user.Nickname} 進行 {AppSettings.DefaultFight.GetDescriptionText()} {response.StatusCode}");
            var content = response.Content;
            if (response.IsSuccessStatusCode)
            {
                //await using var responseStream = await content.ReadAsStreamAsync();
                //await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);
                //var output = await decompressed.ReadAsJsonAsync<GZipStream, BattleLog>();
                var output = await content.ReadAsJsonAsync<BattleLog>();
                Console.WriteLine(output.Messages.ToJsonString());
                Console.WriteLine($"與 [{user.Lv}] {user.Nickname} 戰鬥 {output.Result} 獲得 {output.Gained.Exp} 經驗");
                return output;
            }

            await OnErrorOccur(response.StatusCode, content, "Challenge");
            return null;
        }

        private async Task OnErrorOccur(HttpStatusCode statusCode, HttpContent content, string message = "")
        {
            if (statusCode == HttpStatusCode.Forbidden)
            {
                // 需驗證
                try
                {
                    //await using var responseStream = await content.ReadAsStreamAsync();
                    //await using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);
                    //var output = await decompressed.ReadAsJsonAsync<GZipStream, ErrorOutput>();
                    var output = await content.ReadAsJsonAsync<ErrorOutput>();
                    Console.WriteLine(output.ToJsonString());
                    Console.WriteLine($"驗證後按任意鍵繼續[{AppSettings.MyKiritoDto.Nickname}][{AppSettings.DefaultAct.GetDescriptionText()}] {message}");
                }
                catch
                {
                    var output = await content.ReadAsStringAsync();
                    Console.WriteLine(output);
                    Console.WriteLine("換IP後按任意鍵繼續");
                }
                Console.ReadKey();
            }
            else if (statusCode == HttpStatusCode.BadRequest)
            {
                // 冷卻中or你的角色現在是死亡狀態
                try
                {
                    var output = await content.ReadAsJsonAsync<ErrorOutput>();
                    Console.WriteLine(output.ToJsonString());
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
        }
    }
}