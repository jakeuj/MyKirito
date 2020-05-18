using System.Threading.Tasks;

namespace MyKirito
{
    internal interface IYourKiritoService
    {
        Task<int> StartLoop();
        Task<bool> GetMyKirito();
        Task<bool> DoAction(ActionEnum input);
        Task<bool> ReIncarnation(long freePoints);
        Task<bool> GetUserListThenChallenge();
    }
}