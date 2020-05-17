using System.Linq;
using System.Threading.Tasks;

namespace MyKirito
{
    public class YourKiritoService : IYourKiritoService
    {
        private readonly IMyKiritoService _myKiritoService;

        public YourKiritoService(MyKiritoService myKiritoService)
        {
            _myKiritoService = myKiritoService;
        }

        public MyKirito MyKiritoDto { get; private set; }

        public async Task<bool> GetMyKirito()
        {
            MyKiritoDto = await _myKiritoService.GetMyKirito();
            return MyKiritoDto != null;
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
            if (!string.IsNullOrWhiteSpace(AppSettings.PvpNickName))
            {
                userList = await _myKiritoService.GetUserByName(AppSettings.PvpNickName);
                user = userList?.UserList.FirstOrDefault();
                if (user != null) battleLog = await _myKiritoService.Challenge(user);
            }

            if (MyKiritoDto != null && battleLog == null)
            {
                userList = await _myKiritoService.GetUserListByLevel(MyKiritoDto.Lv + AppSettings.PvpLevel);
                user = userList?.UserList.Where(x=>x.Color!="grey").OrderByDescending(x=>x.Color).ThenByDescending(x=>x.Lv).FirstOrDefault();
                if (user != null) battleLog = await _myKiritoService.Challenge(user);
            }

            return battleLog != null;
        }
    }
}