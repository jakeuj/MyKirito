using System.Net;
using System.Threading.Tasks;

namespace MyKirito
{
    public interface IMyKiritoService
    {
        Task<MyKirito> GetMyKirito();

        Task<ProfileDto> GetProfile(string uid);
        Task<ActionOutput> DoAction(ActionEnum input);
        Task<(BattleLog battleLog, HttpStatusCode statusCode, ErrorOutput errorOutput)> Challenge(long userLv, string userUid, string userNickName);
        Task<string> ReIncarnation(long freePoints);

        Task<UserListDto> GetUserByName(string nickName);
        Task<UserListDto> GetUserListByLevel(long level);
    }
}