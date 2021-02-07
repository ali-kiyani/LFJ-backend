using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using LFJ.Authorization;

namespace LFJ
{
    [DependsOn(
        typeof(LFJCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class LFJApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<LFJAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(LFJApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
