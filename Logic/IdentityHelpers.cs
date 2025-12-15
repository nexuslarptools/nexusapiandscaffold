using System;
using System.Security.Claims;

namespace NEXUSDataLayerScaffold.Logic
{
    public static class IdentityHelpers
    {
        /// <summary>
        /// Extracts the user's email from the authenticated principal.
        /// Order of precedence: ClaimTypes.Email → "email" → "sub".
        /// Returns null when not present.
        /// </summary>
        public static string? GetEmail(ClaimsPrincipal user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return null;

            var email = user.FindFirstValue(ClaimTypes.Email)
                        ?? user.FindFirstValue("email")
                        ?? user.FindFirstValue("sub");

            return string.IsNullOrWhiteSpace(email) ? null : email;
        }
    }
}
