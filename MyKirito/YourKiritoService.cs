using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyKirito
{
    public class YourKiritoService : IYourKiritoService
    {
        private readonly ILogger _logger;
        private readonly IMyKiritoService _myKiritoService;
        private readonly Random RandomCd;
        public YourKiritoService(MyKiritoService myKiritoService, ILogger<YourKiritoService> logger)
        {
            _myKiritoService = myKiritoService;
            RandomCd = new Random();
            _logger = logger;
            _logger.LogDebug(Global.GameOptions.Token);
        }

        public async Task<int> StartLoop()
        {
            // 取得角色資料
            if (await GetMyKirito())
            {
                // 轉生限制條件：滿十等或死亡
                if (Global.MyKiritoDto.Dead || 
                    (Global.GameOptions.DefaultReIncarnationLevel > 0 &&
                    Global.MyKiritoDto.Lv >= Global.GameOptions.DefaultReIncarnationLevel))
                {
                        // 開始轉生
                        if (await ReIncarnation())
                            await CommonLoop();                
                }
                else
                {
                    await CommonLoop();
                }
            }
            int addTime = Global.CheckTime;
            if (Global.GameOptions.DefaultAct == ActionEnum.None)
            {
                addTime = Global.PvpTime;
            }
            if (Global.GameOptions.RandTime > 0)
                addTime += RandomCd.Next(1, Global.GameOptions.RandTime);
            Global.NextActionTime = DateTime.Now.AddSeconds(addTime);
            var output=string.Empty;
            if (Global.MyKiritoDto != null)
                output += $"[{Global.MyKiritoDto.Nickname}][{Global.MyKiritoDto.Lv}], ";
            output += $" 下次戰鬥： {Global.NextPvpTime.ToShortTimeString()}, 下次行動： {Global.NextActionTime.ToShortTimeString()}";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(output);
            Console.ResetColor();
            return addTime;
        }

        public async Task CommonLoop()
        {
            // 日常動作：汁妹之類的
            await DoAction(Global.GameOptions.DefaultAct);
            // PVP 
            if (Global.GameOptions.DefaultFight != FightEnum.None && DateTime.Now > Global.NextPvpTime)
            {
                if (await GetUserListThenChallenge())
                    Global.NextPvpTime = DateTime.Now.AddSeconds(Global.PvpTime);
            }
        }

        public async Task<bool> GetMyKirito()
        {
            Global.MyKiritoDto = await _myKiritoService.GetMyKirito();
            return Global.MyKiritoDto != null;
        }

        public async Task<bool> DoAction(ActionEnum input)
        {
            if (Global.GameOptions.DefaultAct == ActionEnum.None)
                return true;
            var actionOutput = await _myKiritoService.DoAction(input);
            return actionOutput != null;
        }

        public async Task<bool> ReIncarnation()
        {
            if (Global.GameOptions.DefaultChar == CharEnum.None)
            {
                Console.WriteLine("請手動轉生後按任意鍵繼續");
                Console.ReadKey();
                return true;
            }
            // 計算轉生屬性點數
            var freePoints = CheckPoint(Global.MyKiritoDto.Lv);
            var result = await _myKiritoService.ReIncarnation(freePoints);
            return !string.IsNullOrWhiteSpace(result);
        }

        public async Task<bool> GetUserListThenChallenge()
        {
            _logger.LogDebug("GetUserListThenChallenge");
            UserListDto userList;
            UserList user;
            BattleLog battleLog = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.UpgradeRequired;
            ErrorOutput errorOutput=null;
            string coonDownMessage = "還在冷卻中";
            _logger.LogDebug("PvpUid"+ Global.GameOptions.PvpUid);
            if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid))
            {
                var uidUser = await _myKiritoService.GetProfile(Global.GameOptions.PvpUid);
                if (uidUser != null && uidUser.profile != null)
                {
                    if (!uidUser.profile.dead)
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(uidUser.profile.lv, uidUser.profile._id, uidUser.profile.nickname);
                    if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpNickName) && uidUser.profile.nickname == Global.GameOptions.PvpNickName)
                        Global.GameOptions.PvpNickName = string.Empty;
                    if (httpStatusCode == HttpStatusCode.Forbidden || battleLog != null)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                }                
            }
            _logger.LogDebug("PvpUid" + Global.GameOptions.PvpNickName);
            if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpNickName) && battleLog == null)
            {                
                userList = await _myKiritoService.GetUserByName(Global.GameOptions.PvpNickName);
                if(userList!=null && userList.UserList !=null && userList.UserList.Any())
                {
                    user = userList.UserList.FirstOrDefault();
                    if(user!=null)
                    {
                        if(string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid))
                            Global.GameOptions.PvpUid = user.Uid;
                        if (user != null) (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(user.Lv, user.Uid, user.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden || battleLog != null)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                    }                    
                }                
            }
            if (Global.MyKiritoDto == null)
                return false;
            _logger.LogDebug("CurrentPvpUser");
            if (Global.GameOptions.CurrentPvpUser!=null)
            {
                var uidUser = await _myKiritoService.GetProfile(Global.GameOptions.CurrentPvpUser.Uid);
                if (uidUser != null && uidUser.profile != null)
                {
                    if (!uidUser.profile.dead && uidUser.profile.lv >= Global.MyKiritoDto.Lv)
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(uidUser.profile.lv, uidUser.profile._id, uidUser.profile.nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                    {
                        if (battleLog.Result != "勝利")
                        {
                            Global.GameOptions.LostUidListPVP.Add(Global.GameOptions.CurrentPvpUser.Uid);
                            Global.GameOptions.CurrentPvpUser = null;                                             
                        }
                        return true;
                    }
                }
            }
            _logger.LogDebug("MustIsCharacterPVP");
            while (Global.GameOptions.MustIsModeEnable && Global.GameOptions.MustIsCharacterPVP.Any() &&  Global.GameOptions.CurrentSearchLv >= Global.MyKiritoDto.Lv)
            {
                userList = await _myKiritoService.GetUserListByLevel(Global.GameOptions.CurrentSearchLv, Global.GameOptions.CurrentPage);
                if (userList != null && userList.UserList != null && userList.UserList.Any())
                {
                    if (Global.GameOptions.CurrentSearchLv == userList.UserList.Min(x => x.Lv))
                    {
                        Global.GameOptions.CurrentPage++;
                    }
                    else
                    {
                        Global.GameOptions.CurrentSearchLv = userList.UserList.Min(x => x.Lv);
                        Global.GameOptions.CurrentPage = 1;
                    }
                    var users = userList.UserList.Where(x=>x.Color!= "grey" && Global.GameOptions.MustIsCharacterPVP.Contains(x.Character) && !Global.GameOptions.LostUidListPVP.Contains(x.Nickname) && x.Lv>= Global.MyKiritoDto.Lv).OrderByDescending(x => x.Color).OrderByDescending(x=>x.Lv).ThenBy(x => x.Floor).ToList();
                    foreach (var u in users)
                    {
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                        if (battleLog != null)
                        {
                            if (battleLog.Result != "勝利")
                            {
                                Global.GameOptions.CurrentPvpUser = null;
                                Global.GameOptions.LostUidListPVP.Add(u.Uid);                                
                            }
                            else
                            {
                                Global.GameOptions.CurrentPvpUser = u;
                            }                                
                            return true;
                        }                            
                    }
                }
                else
                    break;               
            }
            _logger.LogDebug("GetUserListByLevel"+ Global.MyKiritoDto.Lv + Global.GameOptions.PvpLevel);
            userList = await _myKiritoService.GetUserListByLevel(Global.MyKiritoDto.Lv + Global.GameOptions.PvpLevel);
            if(userList != null && userList.UserList != null && userList.UserList.Any())
            {
                var users = userList.UserList;
                users.RemoveAll(x => x.Color == "grey");
                // 先打紅 橘
                foreach (var u in users.Where(x=> Global.GameOptions.ColorPVP.Contains(x.Color)).OrderByDescending(x => x.Color).ThenBy(x=>x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if(battleLog != null)
                        return true;
                }
                users.RemoveAll(x => Global.GameOptions.ColorPVP.Contains(x.Color));
                // 先打喜歡的人
                foreach (var u in users.Where(x => Global.GameOptions.CharacterPVP.Contains(x.Character)).OrderBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                        return true;
                }
                users.RemoveAll(x => Global.GameOptions.CharacterPVP.Contains(x.Character));
                // 先不打討厭的人
                foreach (var u in users.Where(x => !Global.GameOptions.NotWantCharacterPVP.Contains(x.Character)).OrderBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                        return true;
                }
                users.RemoveAll(x => !Global.GameOptions.NotWantCharacterPVP.Contains(x.Character));
                // 打剩下的人
                foreach (var u in users.OrderBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                        return true;
                }
            }
            
            return battleLog != null;
        }

        //檢查獲得屬性點數
        public long CheckPoint(long input)
        {
            //超過最大等級時：超過幾級就拿幾點 + 所有獎勵等級各拿一點(有幾個限制就拿幾點)
            if (input > Global.GameOptions.AddPointLevel.Max())
                return input - Global.GameOptions.AddPointLevel.Max() + Global.GameOptions.AddPointLevel.Count();
            //否則等級滿足幾個門檻就拿點幾額外屬性點
            return Global.GameOptions.AddPointLevel.Count(x => input >= x);
        }
    }
}