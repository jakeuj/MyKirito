using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                    Global.GameOptions.DefaultReIncarnationLevel > 0 &&
                    Global.MyKiritoDto.Lv >= Global.GameOptions.DefaultReIncarnationLevel)
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

            var addTime = Global.CheckTime;
            if (Global.GameOptions.DefaultAct == ActionEnum.None || Global.GameOptions.DefaultAct == ActionEnum.H1 || Global.GameOptions.DefaultAct == ActionEnum.H2 || Global.GameOptions.DefaultAct == ActionEnum.H4 || Global.GameOptions.DefaultAct == ActionEnum.H8 || Global.GameOptions.DefaultAct == ActionEnum.FloorBonus) addTime = Global.PvpTime;
            if (Global.GameOptions.RandTime > 0)
                addTime += RandomCd.Next(1, Global.GameOptions.RandTime);
            var output = string.Empty;
            if (Global.MyKiritoDto != null)
                output += $"[{Global.MyKiritoDto.Nickname}][{Global.MyKiritoDto.Lv}], ";
            output +=
                $" 下次戰鬥： {Global.NextPvpTime.ToShortTimeString()}, 下次行動： {Global.NextActionTime.ToShortTimeString()}";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(output);
            Console.ResetColor();
            return addTime;
        }

        public async Task CommonLoop()
        {
            // 日常動作：汁妹之類的
            if(DateTime.Now > Global.NextActionTime && await DoAction(Global.GameOptions.DefaultAct))
            {
                int act;
                switch (Global.GameOptions.DefaultAct)
                {
                    case ActionEnum.H1:
                        act = 1 * 60 * 60;
                        break;
                    case ActionEnum.H2:
                        act = 2 * 60 * 60;
                        break;
                    case ActionEnum.H4:
                        act = 4 * 60 * 60;
                        break;
                    case ActionEnum.H8:
                        act = 8 * 60 * 60;
                        break;
                    case ActionEnum.FloorBonus:
                        act = Global.FloorTime * 60 * 60;
                        break;
                    default:
                        act = Global.CheckTime;
                        break;
                }
                Global.NextActionTime = DateTime.Now.AddSeconds(act);
            }
                    
            //樓層獎勵
            if (Global.GameOptions.FloorBonusEnable && Global.GameOptions.DefaultAct!= ActionEnum.FloorBonus && DateTime.Now > Global.NextFloorTime && await DoAction(ActionEnum.FloorBonus))
                Global.NextFloorTime = DateTime.Now.AddHours(Global.FloorTime);                
            // PVP 
                if (Global.GameOptions.DefaultFight != FightEnum.None && DateTime.Now > Global.NextPvpTime && await GetUserListThenChallenge())
                    Global.NextPvpTime = DateTime.Now.AddSeconds(Global.PvpTime);
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
            var httpStatusCode = HttpStatusCode.UpgradeRequired;
            ErrorOutput errorOutput = null;
            var coonDownMessage = "還在冷卻中";
            _logger.LogDebug("PvpUid" + Global.GameOptions.PvpUid);
            if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid))
            {
                var uidUser = await _myKiritoService.GetProfile(Global.GameOptions.PvpUid);
                if (uidUser != null && uidUser.profile != null)
                {
                    if (!uidUser.profile.dead)
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(uidUser.profile.lv,
                            uidUser.profile._id, uidUser.profile.nickname);
                    if (!string.IsNullOrWhiteSpace(Global.GameOptions.PvpNickName) &&
                        uidUser.profile.nickname == Global.GameOptions.PvpNickName)
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
                if (userList != null && userList.UserList != null && userList.UserList.Any())
                {
                    user = userList.UserList.FirstOrDefault();
                    if (user != null)
                    {
                        if (string.IsNullOrWhiteSpace(Global.GameOptions.PvpUid))
                            Global.GameOptions.PvpUid = user.Uid;
                        if (user != null)
                            (battleLog, httpStatusCode, errorOutput) =
                                await _myKiritoService.Challenge(user.Lv, user.Uid, user.Nickname);
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
            if (Global.GameOptions.CurrentPvpUser != null &&
                Global.GameOptions.CurrentPvpUser.Lv > Global.MyKiritoDto.Lv)
            {
                var uidUser = await _myKiritoService.GetProfile(Global.GameOptions.CurrentPvpUser.Uid);
                if (uidUser != null && uidUser.profile != null)
                {
                    if (uidUser.profile.dead || Global.MyKiritoDto.Lv > uidUser.profile.lv)
                    {
                        Global.GameOptions.LostUidListPVP.Add(Global.GameOptions.CurrentPvpUser.Uid);
                        Global.GameOptions.CurrentPvpUser = null;
                    }
                    else
                    {
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(uidUser.profile.lv,
                            uidUser.profile._id, uidUser.profile.nickname);
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
            }

            _logger.LogDebug("MustIsCharacterPVP");
            while (Global.GameOptions.MustIsModeEnable && Global.GameOptions.CurrentSearchLv >= Global.MyKiritoDto.Lv)
            {
                _logger.LogDebug(
                    $"取得目標清單：Lv={Global.GameOptions.CurrentSearchLv}, Page={Global.GameOptions.CurrentPage}");
                userList = await _myKiritoService.GetUserListByLevel(Global.GameOptions.CurrentSearchLv,
                    Global.GameOptions.CurrentPage);
                if (userList == null || userList.UserList == null || !userList.UserList.Any())
                {
                    _logger.LogDebug("清單取得失敗");
                    break;
                }

                var minLv = userList.UserList.Min(x => x.Lv);
                _logger.LogDebug($"紀錄目前清單的最低等級 {minLv}");
                var users = userList.UserList;
                _logger.LogDebug("排除不打的資料並排序");
                users.RemoveAll(x =>
                    x.Color == "grey" || x.Lv < Global.MyKiritoDto.Lv ||
                    Global.GameOptions.LostUidListPVP.Contains(x.Uid));
                if (Global.GameOptions.MustIsModeIgnore == false && Global.GameOptions.MustIsCharacterPVP != null &&
                    Global.GameOptions.MustIsCharacterPVP.Any())
                    users.RemoveAll(x => Global.GameOptions.MustIsCharacterPVP.Contains(x.Character));
                if (!users.Any())
                {
                    _logger.LogDebug("如果沒有資料則直接繼續下一個迴圈取得下一頁");
                    Global.GameOptions.CurrentPage++;
                    continue;
                }

                _logger.LogDebug("篩選後還有剩餘資料，逐一對目標進行挑戰");
                foreach (var u in users)
                {
                    _logger.LogDebug($"{u.Lv} - {u.Uid} - {u.Nickname}");
                    (battleLog, httpStatusCode, errorOutput) =
                        await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    _logger.LogDebug("處理挑戰結果");
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                    {
                        _logger.LogDebug("驗證後紀錄當前玩家並進行冷卻");
                        Global.GameOptions.CurrentPvpUser = u;
                        return true;
                    }

                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                    {
                        _logger.LogDebug("自身冷卻中，紀錄當前玩家並退出");
                        Global.GameOptions.CurrentPvpUser = u;
                        return false;
                    }

                    if (battleLog == null)
                    {
                        _logger.LogDebug("戰鬥未完成，可能死了或重生，加入黑名單後繼續對戰下一個目標");
                        Global.GameOptions.LostUidListPVP.Add(u.Uid);
                        continue;
                    }

                    if (battleLog.Result == "勝利")
                    {
                        _logger.LogDebug("戰鬥完成並勝利，紀錄當前玩家供下次對戰使用");
                        Global.GameOptions.CurrentPvpUser = u;
                    }
                    else
                    {
                        _logger.LogDebug("戰鬥完成並輸了，加入黑名單");
                        Global.GameOptions.CurrentPvpUser = null;
                        Global.GameOptions.LostUidListPVP.Add(u.Uid);
                    }

                    _logger.LogDebug("戰鬥完成後退出");
                    return true;
                }

                _logger.LogDebug("目標清單處理完成");
                if (Global.GameOptions.CurrentSearchLv == minLv)
                {
                    _logger.LogDebug($"最低等級{minLv}等於搜尋等級{Global.GameOptions.CurrentSearchLv}，進行翻頁");
                    Global.GameOptions.CurrentPage++;
                }
                else
                {
                    _logger.LogDebug($"搜尋等級{Global.GameOptions.CurrentSearchLv}高於最低等級{minLv}，搜尋條件更新為最低等級，重置頁次");
                    Global.GameOptions.CurrentSearchLv = minLv;
                    Global.GameOptions.CurrentPage = 1;
                }
            }

            _logger.LogDebug("GetUserListByLevel" + Global.MyKiritoDto.Lv + Global.GameOptions.PvpLevel);
            userList = await _myKiritoService.GetUserListByLevel(Global.MyKiritoDto.Lv + Global.GameOptions.PvpLevel);
            if (userList != null && userList.UserList != null && userList.UserList.Any())
            {
                var users = userList.UserList;
                users.RemoveAll(x => x.Color == "grey");
                // 先打紅 橘
                foreach (var u in users.Where(x => Global.GameOptions.ColorPVP.Contains(x.Color))
                    .OrderByDescending(x => x.Color).ThenBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) =
                        await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                        return true;
                }

                users.RemoveAll(x => Global.GameOptions.ColorPVP.Contains(x.Color));
                // 先打喜歡的人
                foreach (var u in users.Where(x => Global.GameOptions.CharacterPVP.Contains(x.Character))
                    .OrderBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) =
                        await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                    if (battleLog != null)
                        return true;
                }

                users.RemoveAll(x => Global.GameOptions.CharacterPVP.Contains(x.Character));
                // 先不打討厭的人
                foreach (var u in users.Where(x => !Global.GameOptions.NotWantCharacterPVP.Contains(x.Character))
                    .OrderBy(x => x.Floor))
                {
                    (battleLog, httpStatusCode, errorOutput) =
                        await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
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
                    (battleLog, httpStatusCode, errorOutput) =
                        await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
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