using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using LFJ.EntityFrameworkCore;
using LFJ.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace LFJ.Web.Tests
{
    [DependsOn(
        typeof(LFJWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class LFJWebTestModule : AbpModule
    {
        public LFJWebTestModule(LFJEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(LFJWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(LFJWebMvcModule).Assembly);
        }
    }
}