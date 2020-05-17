using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace MyKirito
{
    // 遊戲服務介面
    public interface IMyService
    {
        // 遊戲主畫面操作
        Task<bool> DoAction(ActionEnum input);

        // 遊戲投胎機制
        Task<bool> ReIncarnation(int freePoints);

        // 取得遊戲角色資料
        Task<MyKiritoDto> GetMyKiritoFn();
        Task<bool> GetUserList(int exp);
    }

    // 遊戲服務介面實作類別
    public class MyService : IMyService
    {
        // 發起Http要求
        private readonly IHttpClientFactory _clientFactory;
        private readonly HttpClient _clientkiritoAPI;

        // DI注入
        public MyService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _clientkiritoAPI = _clientFactory.CreateClient("kiritoAPI");
        }

        /// <summary>
        ///     角色轉生
        /// </summary>
        /// <param name="freePoints">升級點數</param>
        /// <returns>結果</returns>
        public async Task<bool> ReIncarnation(int freePoints)
        {
            Console.WriteLine($"ReIncarnation {freePoints}");
            var request = new HttpRequestMessage(HttpMethod.Post, "my-kirito/reincarnation");
            // 將點數點到生命值
            var restLoad =
                $"{{\"character\":\"{AppSettings._defaultChar.ToString().ToLower()}\",\"rattrs\":{{\"hp\":0,\"atk\":0,\"def\":0,\"stm\":0,\"agi\":0,\"spd\":0,\"tec\":0,\"int\":{freePoints},\"lck\":0}},\"useReset\":false}}";
            request.Content = new StringContent(restLoad, Encoding.UTF8, "application/json");
            //var client = _clientFactory.CreateClient("kiritoAPI");
            // 送出轉生請求
            var response = await _clientkiritoAPI.SendAsync(request);
            // 結果
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine($"ReIncarnation {response.StatusCode}");
            }

            return response.IsSuccessStatusCode;
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
                Content = new StringContent($"{{\"action\":\"{input.ToString().ToLower()}{Const.DoActionVersion}\"}}",
                    Encoding.UTF8,
                    "application/json")
            };
            //var client = _clientFactory.CreateClient("kiritoAPI");
            // 送出行動請求
            var response = await _clientkiritoAPI.SendAsync(request);
            // 結果
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine($"DoAction {response.StatusCode}");
            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        ///     取得角色資料
        /// </summary>
        /// <returns>角色物件JSON</returns>
        public async Task<MyKiritoDto> GetMyKiritoFn()
        {
            Console.WriteLine("GetMyKiritoFn  Result: ");
            var request = new HttpRequestMessage(HttpMethod.Get, "my-kirito");
            //var client = _clientFactory.CreateClient("kiritoInfo");
            var response = await _clientkiritoAPI.SendAsync(request);
            // 不成功則空
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GetMyKiritoFn {response.StatusCode}");
                return null;
            }

            // 成功則反序列化後返回角色物件
            var res = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MyKiritoDto>(res,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            // Log角色資料
            Console.WriteLine(JsonSerializer.Serialize(result,
                new JsonSerializerOptions {Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)}));
            return result;
        }

        // 查詢PvP玩家列表
        public async Task<bool> GetUserList(int exp)
        {
            Console.WriteLine($"GetUserList {exp}");
            var isDone = false;
            HttpRequestMessage request;
            HttpResponseMessage response;
            if(!string.IsNullOrWhiteSpace(AppSettings._pvpNickName))
            {
                request = new HttpRequestMessage(HttpMethod.Get, $"search?nickname={AppSettings._pvpNickName}");
                response = await _clientkiritoAPI.SendAsync(request);
                // 結果
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using (var document = JsonDocument.Parse(result))
                    {
                        var root = document.RootElement;
                        var studentsElement = root.GetProperty("userList");
                        foreach (var student in studentsElement.EnumerateArray())
                            if (!student.TryGetProperty("uid", out var gradeElement))
                            {
                                Console.WriteLine($"目標uid錯誤 {gradeElement}");
                            }
                            else if (!student.TryGetProperty("lv", out var lvElement))
                            {
                                Console.WriteLine($"目標lv錯誤 {lvElement}");
                            }
                            else if (!student.TryGetProperty("color", out var colorElement))
                            {
                                Console.WriteLine($"目標color錯誤 {colorElement}");
                            }
                            else if (colorElement.GetString() == "grey")
                            {
                                Console.WriteLine("目標已死亡");
                            }
                            else
                            {
                                var uid = gradeElement.GetString();
                                var lv = lvElement.GetInt32();
                                var State = await Challenge(uid, lv);
                                isDone = State == HttpStatusCode.OK;
                                if (State == HttpStatusCode.OK || State == HttpStatusCode.BadRequest)
                                    break;
                                await Task.Delay(2 * 1000);
                            }
                    }
                }
                else
                {
                    Console.WriteLine($"GetUserList {response.StatusCode}");
                }
            }
            if(!isDone)
            {
                request = new HttpRequestMessage(HttpMethod.Get, $"user-list?exp={exp}");
                response = await _clientkiritoAPI.SendAsync(request);
                // 結果
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using (var document = JsonDocument.Parse(result))
                    {
                        var root = document.RootElement;
                        var studentsElement = root.GetProperty("userList");
                        foreach (var student in studentsElement.EnumerateArray())
                            if (!student.TryGetProperty("uid", out var gradeElement))
                            {
                                Console.WriteLine($"目標uid錯誤 {gradeElement}");
                            }
                            else if (!student.TryGetProperty("lv", out var lvElement))
                            {
                                Console.WriteLine($"目標lv錯誤 {lvElement}");
                            }
                            else if (!student.TryGetProperty("color", out var colorElement))
                            {
                                Console.WriteLine($"目標color錯誤 {colorElement}");
                            }
                            else if (colorElement.GetString() == "grey")
                            {
                                Console.WriteLine("目標已死亡");
                            }
                            else
                            {
                                var uid = gradeElement.GetString();
                                var lv = lvElement.GetInt32();
                                var State = await Challenge(uid, lv);
                                isDone = State == HttpStatusCode.OK;
                                if (State == HttpStatusCode.OK || State == HttpStatusCode.BadRequest)
                                    break;
                                await Task.Delay(2 * 1000);
                            }
                    }
                }
                else
                {
                    Console.WriteLine($"GetUserList {response.StatusCode}");
                }
            }
            return isDone;
        }

        // 出戰Pvp
        private async Task<HttpStatusCode> Challenge(string uid, int lv)
        {
            Console.WriteLine($"Challenge {uid} {lv}");
            var request = new HttpRequestMessage(HttpMethod.Post, "challenge")
            {
                Content = new StringContent(
                    $"{{\"type\":{(int) AppSettings._defaultFight},\"opponentUID\":\"{uid}\",\"shout\":\"\",\"lv\":{lv}}}",
                    Encoding.UTF8,
                    "application/json")
            };
            //var client = _clientFactory.CreateClient("kiritoAPI");
            // 送出行動請求
            var response = await _clientkiritoAPI.SendAsync(request);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetUserList {result}");
            }
            else
            {
                Console.WriteLine($"GetUserList {response.StatusCode}");
            }

            // 結果
            return response.StatusCode;
        }
    }
}