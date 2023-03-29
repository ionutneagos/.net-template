using Domain.Constants;
using System.Security.Claims;
using System.Security.Principal;

namespace Domain.Extensions
{
    public static class PrincipalExtensions
    {
        public static IEnumerable<string?> Roles(this IPrincipal user)
        {
            return ClaimsOfType(user, ClaimTypes.Role);
        }

       
        public static IEnumerable<string?> ClaimsOfType(this IPrincipal user, string claimType)
        {
            if (user.Identity is not ClaimsIdentity) return Array.Empty<string?>();

            return ((ClaimsIdentity)user.Identity).Claims
                                                   .Where(c => c.Type.Equals(claimType))
                                                   .Select(c => c.Value);
        }

        public static string? ClaimOfType(this IPrincipal user, string claimType)
        {
            return ClaimsOfType(user, claimType).FirstOrDefault();
        }

        public static int? GetTenantFromClaim(this IPrincipal user)
        {
            var tenantId = ClaimOfType(user, ContextConfiguration.TenantIdClaim);
            if (string.IsNullOrWhiteSpace(tenantId))
                return null;
            else
                return Convert.ToInt32(tenantId);
        }

        public static string DisplayName(this IPrincipal user)
        {
            string? surname = ClaimOfType(user, ClaimTypes.Surname);
            string? givenName = ClaimOfType(user, ClaimTypes.GivenName);

            if (string.IsNullOrWhiteSpace(surname) && string.IsNullOrWhiteSpace(givenName)) return string.Empty;

            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(givenName)) return string.Format("{0}{1}", givenName ?? string.Empty, surname ?? string.Empty);
            return string.Format("{0} {1}", givenName, surname);
        }
    }
}
