using System.Threading.Tasks;

namespace MyKirito
{
    public interface IMyKiritoService
    {
        Task<MyKirito> GetMyKirito();
        Task<ActionOutput> DoAction(ActionEnum input);
        Task<BattleLog> Challenge(UserList user);
        Task<string> ReIncarnation(long freePoints);

        Task<UserListDto> GetUserByName(string nickName);
        Task<UserListDto> GetUserListByLevel(long level);
    }
}