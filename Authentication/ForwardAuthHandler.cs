using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace NEXUSDataLayerScaffold.Authentication
{
    /// <summary>
    /// Authentication handler that trusts identity headers set by a reverse proxy (ForwardAuth pattern).
    /// If the request already has an authenticated principal with AuthenticationType == "ForwardAuth",
    /// it simply returns Success with that principal. Otherwise, when standard forward-auth headers are
    /// present, it builds a principal from those headers.
    /// </summary>
    public sealed class ForwardAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string Scheme = "ForwardAuth";

        public ForwardAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                  ILoggerFactory logger,
                                  UrlEncoder encoder,
                                  ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // If middleware already created a ForwardAuth principal, reuse it
            if (Context.User?.Identity is ClaimsIdentity id && id.IsAuthenticated &&
                string.Equals(id.AuthenticationType, Scheme, StringComparison.Ordinal))
            {
                var ticket = new AuthenticationTicket(Context.User, Scheme);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            // Prefer ID Token forwarded by the proxy if present (X-Auth-Request-Token)
            var idToken = Request.Headers["X-Auth-Request-Token"].ToString();
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                Logger.LogDebug("ForwardAuth: X-Auth-Request-Token detected; attempting to parse ID token (length={Len})", idToken.Length);
                // Allow optional "Bearer " prefix
                if (idToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    idToken = idToken.Substring("Bearer ".Length).Trim();
                }

                try
                {
                    // Parse token without validating (proxy is trusted in ForwardAuth pattern)
                    // Use lightweight base64url JSON parsing to reliably extract arrays
                    static string B64Url(string s)
                    {
                        s = s.Replace('-', '+').Replace('_', '/');
                        switch (s.Length % 4)
                        {
                            case 2: s += "=="; break;
                            case 3: s += "="; break;
                        }
                        var bytes = Convert.FromBase64String(s);
                        return Encoding.UTF8.GetString(bytes);
                    }

                    var parts = idToken.Split('.');
                    if (parts.Length >= 2)
                    {
                        var payloadJson = B64Url(parts[1]);
                        using var doc = JsonDocument.Parse(payloadJson);
                        var root = doc.RootElement;

                        var tokenIdentity = new ClaimsIdentity(Scheme);
                        if (root.TryGetProperty("sub", out var sub))
                        {
                            var s = sub.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                tokenIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, s!));
                                tokenIdentity.AddClaim(new Claim("sub", s!));
                            }
                        }
                        if (root.TryGetProperty("name", out var nameEl))
                        {
                            var n = nameEl.GetString();
                            if (!string.IsNullOrWhiteSpace(n))
                            {
                                tokenIdentity.AddClaim(new Claim(ClaimTypes.Name, n!));
                                tokenIdentity.AddClaim(new Claim("name", n!));
                            }
                        }
                        if (root.TryGetProperty("email", out var emailEl))
                        {
                            var e = emailEl.GetString();
                            if (!string.IsNullOrWhiteSpace(e))
                            {
                                tokenIdentity.AddClaim(new Claim(ClaimTypes.Email, e!));
                                tokenIdentity.AddClaim(new Claim("email", e!));
                            }
                        }

                        // Roles: prefer new namespace; accept legacy namespaces and plain 'roles' array (case-insensitive)
                        var canonical = new[] { "Reader", "Writer", "Approver", "HeadGM", "Wizard" };

                        void AddRoleValues(JsonElement arr, string? mirrorClaimType)
                        {
                            if (arr.ValueKind != JsonValueKind.Array) return;
                            foreach (var r in arr.EnumerateArray())
                            {
                                var rv = r.GetString();
                                if (string.IsNullOrWhiteSpace(rv)) continue;
                                // Add as role
                                if (!tokenIdentity.HasClaim(ClaimTypes.Role, rv!))
                                    tokenIdentity.AddClaim(new Claim(ClaimTypes.Role, rv!));
                                // Mirror original claim shape for compatibility
                                if (!string.IsNullOrEmpty(mirrorClaimType))
                                    tokenIdentity.AddClaim(new Claim(mirrorClaimType!, rv!));
                                // Add canonical variant if applicable
                                var canon = canonical.FirstOrDefault(x => string.Equals(x, rv, StringComparison.OrdinalIgnoreCase));
                                if (canon != null && !tokenIdentity.HasClaim(ClaimTypes.Role, canon))
                                    tokenIdentity.AddClaim(new Claim(ClaimTypes.Role, canon));
                            }
                        }

                        // Try new namespace first
                        if (root.TryGetProperty("https://Nexuslarp.com/roles", out var nsRolesNew))
                        {
                            AddRoleValues(nsRolesNew, "https://Nexuslarp.com/roles");
                        }
                        else if (root.TryGetProperty("roles", out var roles))
                        {
                            AddRoleValues(roles, "roles");
                        }
                        else
                        {
                            // Search for legacy namespaces case-insensitively
                            foreach (var prop in root.EnumerateObject())
                            {
                                if (string.Equals(prop.Name, "https://nexuslarps.com/roles", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(prop.Name, "https://NexusLarps.com/roles", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddRoleValues(prop.Value, prop.Name);
                                    break;
                                }
                            }
                        }

                        // Log summary of extracted claims (mask PII outside Development)
                        try
                        {
                            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                            var dev = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
                            string Mask(string? s) => string.IsNullOrWhiteSpace(s) ? s ?? string.Empty : (s!.Length <= 3 ? "***" : s.Substring(0, 3) + "***");
                            string MaskEmail(string? s)
                            {
                                if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
                                var at = s.IndexOf('@');
                                if (at <= 1) return "***@***";
                                return s.Substring(0, 1) + "***" + s.Substring(at);
                            }

                            var subVal = tokenIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? tokenIdentity.FindFirst("sub")?.Value;
                            var emailVal = tokenIdentity.FindFirst(ClaimTypes.Email)?.Value ?? tokenIdentity.FindFirst("email")?.Value;
                            var rolesVals = tokenIdentity.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                            if (!dev)
                            {
                                subVal = Mask(subVal);
                                emailVal = MaskEmail(emailVal);
                            }
                            Logger.LogInformation("ForwardAuth(IDToken): sub={Sub}, email={Email}, rolesCount={Count}, roles={Roles}", subVal, emailVal, rolesVals.Length, rolesVals);
                        }
                        catch { /* ignore logging errors */ }

                        var principalFromToken = new ClaimsPrincipal(tokenIdentity);
                        var ticketFromToken = new AuthenticationTicket(principalFromToken, Scheme);
                        return Task.FromResult(AuthenticateResult.Success(ticketFromToken));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to parse ID token from X-Auth-Request-Token header");
                    // fall through to header-based mapping
                }
            }

            // Otherwise, attempt to build a principal from common forward-auth headers
            var headers = Request.Headers;
            bool Has(string key) => headers.ContainsKey(key) && !string.IsNullOrWhiteSpace(headers[key].ToString());

            var hasAny = Has("X-Forwarded-Email") || Has("X-Auth-Request-Email") ||
                         Has("X-Forwarded-User") || Has("X-Auth-Request-User") ||
                         Has("X-Forwarded-Subject") || Has("X-Auth-Request-Userid") ||
                         Has("X-Forwarded-Groups") || Has("X-Auth-Request-Groups") ||
                         Has("X-User-Roles");

            if (!hasAny)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string? Get(string key) => headers.TryGetValue(key, out var v) ? v.ToString() : null;

            var email = Get("X-Forwarded-Email") ?? Get("X-Auth-Request-Email");
            var user = Get("X-Forwarded-User") ?? Get("X-Auth-Request-User");
            var subject = Get("X-Forwarded-Subject") ?? Get("X-Auth-Request-Userid") ?? user ?? email;
            var groupsHeader = Get("X-Forwarded-Groups") ?? Get("X-Auth-Request-Groups");
            var rolesHeader = Get("X-User-Roles");

            var identity = new ClaimsIdentity(Scheme);
            if (!string.IsNullOrWhiteSpace(subject))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subject!));
                identity.AddClaim(new Claim("sub", subject!));
            }
            if (!string.IsNullOrWhiteSpace(user))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, user!));
                identity.AddClaim(new Claim("name", user!));
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email!));
                identity.AddClaim(new Claim("email", email!));
            }

            // Support roles coming from either Groups headers or X-User-Roles (comma/semicolon separated)
            void AddDelimitedRoles(string? raw, string mirrorClaimType)
            {
                if (string.IsNullOrWhiteSpace(raw)) return;
                var items = raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var item in items)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, item));
                    // Mirror the original source as a raw claim to help diagnostics/transform
                    identity.AddClaim(new Claim(mirrorClaimType, item));
                }
            }

            AddDelimitedRoles(groupsHeader, "groups");
            AddDelimitedRoles(rolesHeader, "roles");

            // Log summary for header-based auth
            try
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var dev = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
                string Mask(string? s) => string.IsNullOrWhiteSpace(s) ? s ?? string.Empty : (s!.Length <= 3 ? "***" : s.Substring(0, 3) + "***");
                string MaskEmail(string? s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
                    var at = s.IndexOf('@');
                    if (at <= 1) return "***@***";
                    return s.Substring(0, 1) + "***" + s.Substring(at);
                }
                var subVal = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? identity.FindFirst("sub")?.Value;
                var emailVal = identity.FindFirst(ClaimTypes.Email)?.Value ?? identity.FindFirst("email")?.Value;
                var rolesVals = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                if (!dev)
                {
                    subVal = Mask(subVal);
                    emailVal = MaskEmail(emailVal);
                }
                Logger.LogInformation("ForwardAuth(Headers): sub={Sub}, email={Email}, rolesCount={Count}, roles={Roles}", subVal, emailVal, rolesVals.Length, rolesVals);
            }
            catch { /* ignore */ }

            var principal = new ClaimsPrincipal(identity);
            var ticket2 = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket2));
        }
    }
}
