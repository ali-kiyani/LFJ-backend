using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace LFJ.Authorization
{
    public class LFJAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            context.CreatePermission(PermissionNames.Pages_Products, L("Products"));
            context.CreatePermission(PermissionNames.Pages_Orders, L("Orders"));
            context.CreatePermission(PermissionNames.Pages_Reservations, L("Reservations"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, LFJConsts.LocalizationSourceName);
        }
    }
}
