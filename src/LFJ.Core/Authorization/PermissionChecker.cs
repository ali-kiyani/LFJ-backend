using Abp.Authorization;
using LFJ.Authorization.Roles;
using LFJ.Authorization.Users;

namespace LFJ.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
