using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace LFJ.Controllers
{
    public abstract class LFJControllerBase: AbpController
    {
        protected LFJControllerBase()
        {
            LocalizationSourceName = LFJConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
