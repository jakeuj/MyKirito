using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyKirito
{
    public class YourKiritoService : IYourKiritoService
    {
        private readonly IMyKiritoService _myKiritoService;
        private readonly Random RandomCd;
        public YourKiritoService(MyKiritoService myKiritoService)
        {
            _myKiritoService = myKiritoService;
            RandomCd = new Random();
        }

        public async Task<int> StartLoop()
        {
            // 取得角色資料
            if (await GetMyKirito())
            {
                // 轉生限制條件：滿十等或死亡
                if (AppSettings.MyKiritoDto.Dead || AppSettings.DefaultReIncarnationLevel > 0 &&
                    AppSettings.MyKiritoDto.Lv >= AppSettings.DefaultReIncarnationLevel)
                {
                    // 計算轉生屬性點數
                    var freePoints = CheckPoint(AppSettings.MyKiritoDto.Lv);
                    // 開始轉生
                    if(await ReIncarnation(freePoints))
                        await CommonLoop();
                }
                else
                {
                    await CommonLoop();
                }                
            }
            int addTime = Const.CheckTime;
            if (AppSettings.RandTime > 0)
                addTime += RandomCd.Next(1, AppSettings.RandTime);
            AppSettings.NextActionTime = DateTime.Now.AddSeconds(addTime);

            var output = DateTime.Now.ToShortTimeString();
            if (AppSettings.MyKiritoDto != null)
                output += $" [{AppSettings.MyKiritoDto.Lv}] {AppSettings.MyKiritoDto.Nickname}, ";
            output += $" 下次戰鬥： {AppSettings.NextPvpTime.ToShortTimeString()}, 下次行動： {AppSettings.NextActionTime.ToShortTimeString()}";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(output);
            Console.ResetColor();
            return addTime;
        }

        public async Task CommonLoop()
        {
            // 日常動作：汁妹之類的
            if (await DoAction(AppSettings.DefaultAct))
                // PVP 
                if (AppSettings.DefaultFight != FightEnum.None && DateTime.Now > AppSettings.NextPvpTime)
                    if (await GetUserListThenChallenge())
                        AppSettings.NextPvpTime = DateTime.Now.AddSeconds(Const.PvpTime);
        }

        public async Task<bool> GetMyKirito()
        {
            AppSettings.MyKiritoDto = await _myKiritoService.GetMyKirito();
            return AppSettings.MyKiritoDto != null;
        }

        public async Task<bool> DoAction(ActionEnum input)
        {
            var actionOutput = await _myKiritoService.DoAction(input);
            return actionOutput != null;
        }

        public async Task<bool> ReIncarnation(long freePoints)
        {
            var result = await _myKiritoService.ReIncarnation(freePoints);
            return !string.IsNullOrWhiteSpace(result);
        }

        public async Task<bool> GetUserListThenChallenge()
        {
            bool stopFlag = false;
            UserListDto userList;
            UserList user;
            BattleLog battleLog = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.UpgradeRequired;
            ErrorOutput errorOutput=null;
            string coonDownMessage = "還在冷卻中";
            if (!string.IsNullOrWhiteSpace(AppSettings.PvpUid))
            {
                var uidUser = await _myKiritoService.GetProfile(AppSettings.PvpUid);
                if (uidUser != null && uidUser.profile != null)
                {
                    if (!uidUser.profile.dead)
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(uidUser.profile.lv, uidUser.profile._id, uidUser.profile.nickname);
                    if (!string.IsNullOrWhiteSpace(AppSettings.PvpNickName) && uidUser.profile.nickname == AppSettings.PvpNickName)
                        AppSettings.PvpNickName = string.Empty;
                    if (httpStatusCode == HttpStatusCode.Forbidden)
                        return true;
                    if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                        return false;
                }                
            }
            if (!string.IsNullOrWhiteSpace(AppSettings.PvpNickName) && battleLog == null)
            {                
                userList = await _myKiritoService.GetUserByName(AppSettings.PvpNickName);
                if(userList!=null && userList.UserList !=null && userList.UserList.Any())
                {
                    user = userList.UserList.FirstOrDefault();
                    if(user!=null)
                    {
                        if(string.IsNullOrWhiteSpace(AppSettings.PvpUid))
                            AppSettings.PvpUid = user.Uid;
                        if (user != null) (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(user.Lv, user.Uid, user.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                    }                    
                }                
            }
            if ((errorOutput == null || !errorOutput.Error.Contains(coonDownMessage)) && httpStatusCode != HttpStatusCode.Forbidden && AppSettings.MyKiritoDto != null && battleLog == null)
            {
                userList = await _myKiritoService.GetUserListByLevel(AppSettings.MyKiritoDto.Lv + AppSettings.PvpLevel);
                if(userList != null && userList.UserList != null && userList.UserList.Any())
                {
                    var users = userList.UserList;
                    users.RemoveAll(x => x.Color == "grey");
                    // 先打紅 橘
                    foreach (var u in users.Where(x=> AppSettings.ColorPVP.Contains(x.Color)).OrderBy(x => x.Color).ThenBy(x=>x.Floor))
                    {
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                        if(battleLog != null)
                            return true;
                    }
                    users.RemoveAll(x => AppSettings.ColorPVP.Contains(x.Color));
                    // 先打喜歡的人
                    foreach (var u in users.Where(x => AppSettings.CharacterPVP.Contains(x.Character)).OrderBy(x => x.Floor))
                    {
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                        if (battleLog != null)
                            return true;
                    }
                    users.RemoveAll(x => AppSettings.CharacterPVP.Contains(x.Character));
                    // 先不打討厭的人
                    foreach (var u in users.Where(x => !AppSettings.NotWantCharacterPVP.Contains(x.Character)).OrderBy(x => x.Floor))
                    {
                        (battleLog, httpStatusCode, errorOutput) = await _myKiritoService.Challenge(u.Lv, u.Uid, u.Nickname);
                        if (httpStatusCode == HttpStatusCode.Forbidden)
                            return true;
                        if (errorOutput != null && errorOutput.Error.Contains(coonDownMessage))
                            return false;
                        if (battleLog != null)
                            return true;
                    }
                    users.RemoveAll(x => !AppSettings.NotWantCharacterPVP.Contains(x.Character));
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
            }
            return battleLog != null;
        }

        //檢查獲得屬性點數
        public static long CheckPoint(long input)
        {
            //超過最大等級時：超過幾級就拿幾點 + 所有獎勵等級各拿一點(有幾個限制就拿幾點)
            if (input > AppSettings.AddPointLevel.Max())
                return input - AppSettings.AddPointLevel.Max() + AppSettings.AddPointLevel.Count();
            //否則等級滿足幾個門檻就拿點幾額外屬性點
            return AppSettings.AddPointLevel.Count(x => input >= x);
        }
    }
}