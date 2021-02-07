using System.Threading.Tasks;
using Abp.Application.Services;
using LFJ.Authorization.Accounts.Dto;

namespace LFJ.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
