using System.Threading.Tasks;
using Abp.Application.Services;
using LFJ.Sessions.Dto;

namespace LFJ.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
