using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

            // Otherwise, attempt to build a principal from common forward-auth headers
            var headers = Request.Headers;
            bool Has(string key) => headers.ContainsKey(key) && !string.IsNullOrWhiteSpace(headers[key].ToString());

            var hasAny = Has("X-Forwarded-Email") || Has("X-Auth-Request-Email") ||
                         Has("X-Forwarded-User") || Has("X-Auth-Request-User") ||
                         Has("X-Forwarded-Subject") || Has("X-Auth-Request-Userid") ||
                         Has("X-Forwarded-Groups") || Has("X-Auth-Request-Groups");

            if (!hasAny)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string? Get(string key) => headers.TryGetValue(key, out var v) ? v.ToString() : null;

            var email = Get("X-Forwarded-Email") ?? Get("X-Auth-Request-Email");
            var user = Get("X-Forwarded-User") ?? Get("X-Auth-Request-User");
            var subject = Get("X-Forwarded-Subject") ?? Get("X-Auth-Request-Userid") ?? user ?? email;
            var groupsHeader = Get("X-Forwarded-Groups") ?? Get("X-Auth-Request-Groups");

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

            if (!string.IsNullOrWhiteSpace(groupsHeader))
            {
                var groups = groupsHeader.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var g in groups)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, g));
                    identity.AddClaim(new Claim("groups", g));
                }
            }

            var principal = new ClaimsPrincipal(identity);
            var ticket2 = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket2));
        }
    }
}
