using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Web.Providers
{
    public class TenantAccessRequirement : AuthorizationHandler<TenantAccessRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantAccessRequirement requirement)
        {
            var roles = new[] { IdentityConfiguration.RootRole };

            var userIsInRole = roles.Any(context.User.IsInRole);
            if (userIsInRole)
            {
                context.Fail();
                return Task.FromResult(false);
            }

            context.Succeed(requirement);
            return Task.FromResult(true);
        }
    }
}
