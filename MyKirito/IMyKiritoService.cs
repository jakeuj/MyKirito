using System.Threading.Tasks;

namespace MyKirito
{
    public interface IMyKiritoService
    {
        Task<MyKirito> GetMyKirito();

        Task<ProfileDto> GetProfile(string uid);
        Task<ActionOutput> DoAction(ActionEnum input);
        Task<BattleLog> Challenge(long userLv, string userUid, string userNickName);
        Task<string> ReIncarnation(long freePoints);

        Task<UserListDto> GetUserByName(string nickName);
        Task<UserListDto> GetUserListByLevel(long level);
    }
}