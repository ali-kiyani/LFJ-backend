using Abp.Application.Services;
using LFJ.MultiTenancy.Dto;

namespace LFJ.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

