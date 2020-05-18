using System;
using System.Linq;
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
            UserListDto userList;
            UserList user;
            BattleLog battleLog = null;
            if (!string.IsNullOrWhiteSpace(AppSettings.PvpUid))
            {
                var uidUser = await _myKiritoService.GetProfile(AppSettings.PvpUid);
                if(uidUser!=null && !uidUser.profile.dead)
                    battleLog = await _myKiritoService.Challenge(uidUser.profile.lv, uidUser.profile._id, uidUser.profile.nickname);
            }
            if (!string.IsNullOrWhiteSpace(AppSettings.PvpNickName) && battleLog == null)
            {
                userList = await _myKiritoService.GetUserByName(AppSettings.PvpNickName);
                user = userList?.UserList.FirstOrDefault();
                if (user != null) battleLog = await _myKiritoService.Challenge(user.Lv,user.Uid,user.Nickname);
            }
            if (AppSettings.MyKiritoDto != null && battleLog == null)
            {
                userList = await _myKiritoService.GetUserListByLevel(AppSettings.MyKiritoDto.Lv + AppSettings.PvpLevel);
                user = userList?.UserList.Where(x=>x.Color!="grey").OrderByDescending(x=>x.Color).ThenByDescending(x=>x.Lv).FirstOrDefault();
                if (user != null) battleLog = await _myKiritoService.Challenge(user.Lv, user.Uid, user.Nickname);
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