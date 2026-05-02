using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCoreMvcDemoApp.Core.Web.Filters
{
    using Authentication.Abstractions;

    public class UserAuthorizationFilter(IUserContext userContext, RoleTypes[] roles) : IAuthorizationFilter
    {
        private readonly RoleTypes[] _roles = roles.Contains(RoleTypes.Admin) ? roles : [.. roles, RoleTypes.Admin];

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            userContext.CheckAuthorization(context.HttpContext.User);

            if (!userContext.IsAuthorized)
            {
                throw new UserNotFoundException();
            }

            if (!_roles.Contains(userContext.Role))
            {
                throw new UserNotAllowedException();
            }
        }
    }
}
