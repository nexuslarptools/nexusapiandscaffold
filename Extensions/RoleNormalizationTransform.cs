using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace NEXUSDataLayerScaffold
{
    public sealed class RoleNormalizationTransform : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity is not ClaimsIdentity id || !id.IsAuthenticated)
            {
                return Task.FromResult(principal!);
            }

            // Gather roles from both namespaced and non-namespaced claim sources (backward compatible, case-insensitive)
            // Primary namespace: https://Nexuslarp.com/roles
            // Legacy namespaces occasionally seen: https://nexuslarps.com/roles, https://NexusLarps.com/roles
            var roleTypeCandidates = new[]
            {
                "roles",
                "https://Nexuslarp.com/roles",
                "https://nexuslarps.com/roles",
                "https://NexusLarps.com/roles"
            };

            var roles = id.Claims
                .Where(c => !string.IsNullOrWhiteSpace(c.Value) && roleTypeCandidates.Any(t => string.Equals(c.Type, t, StringComparison.OrdinalIgnoreCase)))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Known canonical roles used in policies (match case exactly as policies require)
            var canonical = new[] { "Reader", "Writer", "Approver", "HeadGM", "Wizard" };

            foreach (var role in roles)
            {
                // Add the role as-is
                if (!id.HasClaim(ClaimTypes.Role, role)) id.AddClaim(new Claim(ClaimTypes.Role, role));

                // Also add a canonical-cased variant when applicable so policy checks succeed even if the token uses different casing
                var canonicalMatch = canonical.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
                if (canonicalMatch != null && !id.HasClaim(ClaimTypes.Role, canonicalMatch))
                {
                    id.AddClaim(new Claim(ClaimTypes.Role, canonicalMatch));
                }
            }

            return Task.FromResult(principal);
        }
    }
}
