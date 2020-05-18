using System.Threading.Tasks;

namespace MyKirito
{
    internal interface IYourKiritoService
    {
        Task<bool> GetMyKirito();
        Task<bool> DoAction(ActionEnum input);
        Task<bool> ReIncarnation(long freePoints);
        Task<bool> GetUserListThenChallenge();
    }
}