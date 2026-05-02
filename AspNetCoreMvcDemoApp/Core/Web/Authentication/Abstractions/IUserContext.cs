using AspNetCoreMvcDemoApp.Core.Common;
using AspNetCoreMvcDemoApp.Models.Abstractions;
using System.Security.Claims;

namespace AspNetCoreMvcDemoApp.Core.Web.Authentication.Abstractions
{
    public interface IUserContext
    {
        /// <summary>
        /// Gets whether the current user is authenticated.
        /// </summary>
        bool IsAuthorized { get; }

        /// <summary>
        /// Gets the authorization role of the current user.
        /// </summary>
        RoleTypes Role { get; }

        /// <summary>
        /// Gets the current authenticated user data.
        /// Throws InvalidOperationException if the user is not authenticated.
        /// </summary>
        AuthUser Current { get; }

        /// <summary>
        /// Provides an ID for database ownership filtering.
        /// Returns 'null' for Admins (no restriction) or the User's ID for restricted access.
        /// </summary>
        int? RestrictionId { get; }

        /// <summary>
        /// Resolves user claims and populates the service properties.
        /// </summary>
        void CheckAuthorization(ClaimsPrincipal user);
    }
}
