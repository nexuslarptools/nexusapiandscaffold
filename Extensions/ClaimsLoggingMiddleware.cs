using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NEXUSDataLayerScaffold.Extensions
{
    /// <summary>
    /// Logs a concise summary of the authenticated principal's claims for troubleshooting.
    /// Placed after UseAuthentication and before UseAuthorization.
    /// Avoids logging raw tokens; masks PII in non-Development environments.
    /// </summary>
    public sealed class ClaimsLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClaimsLoggingMiddleware> _logger;

        public ClaimsLoggingMiddleware(RequestDelegate next, ILogger<ClaimsLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var verbose = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);

            if (user?.Identity?.IsAuthenticated == true)
            {
                var authType = user.Identity.AuthenticationType ?? "(unknown)";
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                var name = user.Identity.Name ?? user.FindFirst("name")?.Value;
                var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                if (!verbose)
                {
                    sub = Mask(sub);
                    name = Mask(name);
                    email = MaskEmail(email);
                }

                _logger.LogInformation(
                    "Authenticated principal. authType={AuthType}, sub={Sub}, name={Name}, email={Email}, rolesCount={RolesCount}, roles={Roles}",
                    authType, sub, name, email, roles.Length, roles);

                // Summarize claim types and role claim sources for debugging normalization
                try
                {
                    var typeCounts = user.Claims
                        .GroupBy(c => c.Type)
                        .ToDictionary(g => g.Key, g => g.Count());
                    _logger.LogDebug("Claim types present: {ClaimTypes}", typeCounts);

                    var roleSources = user.Claims
                        .Where(c => string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase) || c.Type.EndsWith("/roles", StringComparison.OrdinalIgnoreCase))
                        .GroupBy(c => c.Type)
                        .ToDictionary(g => g.Key, g => g.Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
                    _logger.LogDebug("Role claim sources: {RoleSources}", roleSources);
                }
                catch
                {
                    // ignore logging summary errors
                }
            }
            else
            {
                // If forward-auth signals exist but no principal, note it for diagnostics
                bool Has(string k) => context.Request.Headers.ContainsKey(k) && !string.IsNullOrWhiteSpace(context.Request.Headers[k]);
                var hasForwardSignals = Has("X-Auth-Request-Token") || Has("X-Forwarded-Email") || Has("X-Forwarded-User") || Has("X-Forwarded-Subject");
                if (hasForwardSignals)
                {
                    _logger.LogWarning("No authenticated principal, but forward-auth headers detected for path {Path}", context.Request.Path);
                }
            }

            await _next(context);
        }

        private static string Mask(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input ?? string.Empty;
            return input.Length <= 3 ? "***" : input.Substring(0, 3) + "***";
        }

        private static string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return email ?? string.Empty;
            var at = email.IndexOf('@');
            if (at <= 1) return "***@***";
            return email.Substring(0, 1) + "***" + email.Substring(at);
        }
    }
}
