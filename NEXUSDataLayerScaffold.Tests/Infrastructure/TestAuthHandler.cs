using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NEXUSDataLayerScaffold.Tests.Infrastructure;

// Simple authentication handler for tests. It creates an authenticated principal
// based on request headers:
// - X-Test-Sub => sets 'sub' and NameIdentifier
// - X-Test-Roles => adds non-namespaced roles claim array entries ("roles")
// - X-Test-Roles-Namespace => adds namespaced roles claim entries ("https://nexuslarps.com/roles")
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string DefaultScheme = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // If no test subject is provided, treat as unauthenticated
        var sub = Request.Headers["X-Test-Sub"].ToString();
        if (string.IsNullOrWhiteSpace(sub))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity(DefaultScheme);
        identity.AddClaim(new Claim("sub", sub));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sub));

        // Support both claim shapes
        var plainRoles = Request.Headers["X-Test-Roles"].ToString();
        if (!string.IsNullOrWhiteSpace(plainRoles))
        {
            foreach (var r in plainRoles.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
            {
                identity.AddClaim(new Claim("roles", r));
            }
        }

        var nsRoles = Request.Headers["X-Test-Roles-Namespace"].ToString();
        if (!string.IsNullOrWhiteSpace(nsRoles))
        {
            foreach (var r in nsRoles.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
            {
                identity.AddClaim(new Claim("https://nexuslarps.com/roles", r));
            }
        }

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DefaultScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
