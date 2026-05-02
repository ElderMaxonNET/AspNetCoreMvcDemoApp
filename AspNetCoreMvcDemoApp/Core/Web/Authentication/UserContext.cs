using AspNetCoreMvcDemoApp.Core.Infrastructure.Caching.Abstractions;
using System.Security.Claims;

namespace AspNetCoreMvcDemoApp.Core.Web.Authentication
{
    public class UserContext(IActiveUserCache userCache) : IUserContext
    {
        public bool IsAuthorized { get; private set; }
        public RoleTypes Role { get; private set; } = RoleTypes.None;

        private AuthUser? _Current = null;
        public AuthUser Current
        {
            get
            {
                if (_Current == null)
                {
                    throw new InvalidOperationException("Invalid User or user not detected.");
                }
                return _Current;
            }
        }

        private int? _restrictionId = null;
        public int? RestrictionId
        {
            get
            {
                return _restrictionId;
            }
        }

        public void CheckAuthorization(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return;

            RoleTypes tempRole = RoleTypes.None;
            int id = 0, roleId = 0;
            string? email = null, fullName = null, avatar = null;

            foreach (var claim in user.Claims)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                        _ = int.TryParse(claim.Value, out id); break;
                    case ClaimTypes.Email:
                        email = claim.Value; break;
                    case ClaimTypes.Name:
                        fullName = claim.Value; break;
                    case ClaimTypes.Role:
                        if (Enum.TryParse<RoleTypes>(claim.Value, out var parsedRole))
                        {
                            tempRole = parsedRole;
                        }
                        break;
                    case "RoleId":
                        _ = int.TryParse(claim.Value, out roleId); break;
                    case "Avatar":
                        avatar = claim.Value; break;
                }
            }

            if (id > 0 &&
                roleId > 0 &&
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(fullName) &&
                !string.IsNullOrWhiteSpace(avatar) &&
                tempRole != RoleTypes.None &&
                userCache.IsActive(id))
            {
                Role = tempRole;
                _Current = new AuthUser(id, email, fullName, roleId, avatar);
                _restrictionId = Role != RoleTypes.Admin ? Current.Id : null;
                IsAuthorized = true;
            }

        }

    }
}
