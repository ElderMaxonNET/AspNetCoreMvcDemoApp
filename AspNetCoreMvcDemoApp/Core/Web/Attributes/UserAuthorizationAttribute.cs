using AspNetCoreMvcDemoApp.Core.Common;
using AspNetCoreMvcDemoApp.Core.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreMvcDemoApp.Core.Web.Attributes
{
    public class UserAuthorizationAttribute : TypeFilterAttribute
    {
        public UserAuthorizationAttribute(params RoleTypes[] roles): base(typeof(UserAuthorizationFilter))
        {
            Arguments = [roles];
        }
    }
}
