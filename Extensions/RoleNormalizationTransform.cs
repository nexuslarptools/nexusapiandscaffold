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

            // Gather roles from both namespaced and non-namespaced claim sources
            // Updated namespace per ForwardAuth provider: https://Nexuslarp.com/roles
            var roles = id.FindAll("https://Nexuslarp.com/roles").Select(c => c.Value)
                .Concat(id.FindAll("roles").Select(c => c.Value))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var role in roles)
            {
                if (!id.HasClaim(ClaimTypes.Role, role))
                {
                    id.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return Task.FromResult(principal);
        }
    }
}
