using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace EmployeeManagement.Security
{
    public class SuperAdminHandler : AuthorizationHandler<MangeAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MangeAdminRolesAndClaimsRequirement requirement)
        {
            if(context.User.IsInRole("Super Admin")) { context.Succeed(requirement); }
            return Task.CompletedTask;
        }
    }
}
