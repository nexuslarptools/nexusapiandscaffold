using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NEXUSDataLayerScaffold.Extensions
{
    // Middleware compatible with traefikoidc (https://github.com/lukaszraczylo/traefikoidc)
    // Maps forwarded identity headers to an authenticated ClaimsPrincipal when no Authorization
    // header is present and optionally forwards the proxied Authorization header.
    public class ForwardAuthClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public ForwardAuthClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If there is no Authorization header but proxy supplied one, forward it (some setups use X-Forwarded-Authorization)
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var fAuth = context.Request.Headers["X-Forwarded-Authorization"].FirstOrDefault()
                           ?? context.Request.Headers["X-Auth-Request-Authorization"].FirstOrDefault();
                var fAccess = context.Request.Headers["X-Forwarded-Access-Token"].FirstOrDefault()
                            ?? context.Request.Headers["X-Auth-Request-Access-Token"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(fAuth))
                {
                    context.Request.Headers["Authorization"] = fAuth;
                }
                else if (!string.IsNullOrWhiteSpace(fAccess))
                {
                    context.Request.Headers["Authorization"] = $"Bearer {fAccess}";
                }
            }

            // Only create a principal if there is still no Authorization header and the proxy supplied identity headers
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                // traefikoidc commonly sets these headers (depending on provider and configuration)
                var user = context.Request.Headers["X-Forwarded-User"].FirstOrDefault()
                          ?? context.Request.Headers["X-Forwarded-Preferred-Username"].FirstOrDefault()
                          ?? context.Request.Headers["X-Auth-Request-User"].FirstOrDefault();

                // subject/user id
                var subject = context.Request.Headers["X-Forwarded-Subject"].FirstOrDefault()
                           ?? context.Request.Headers["X-Forwarded-UserId"].FirstOrDefault()
                           ?? context.Request.Headers["X-Forwarded-Preferred-Username"].FirstOrDefault()
                           ?? context.Request.Headers["X-Forwarded-User"].FirstOrDefault()
                           ?? context.Request.Headers["X-Auth-Request-Userid"].FirstOrDefault();

                var email = context.Request.Headers["X-Forwarded-Email"].FirstOrDefault()
                         ?? context.Request.Headers["X-Auth-Request-Email"].FirstOrDefault();

                var groupsHeader = context.Request.Headers["X-Forwarded-Groups"].FirstOrDefault()
                                 ?? context.Request.Headers["X-Auth-Request-Groups"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(user) || !string.IsNullOrWhiteSpace(subject) || !string.IsNullOrWhiteSpace(email))
                {
                    var identity = new ClaimsIdentity("ForwardAuth");
                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subject));
                        identity.AddClaim(new Claim("sub", subject));
                    }
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, user));
                        identity.AddClaim(new Claim("name", user));
                    }
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Email, email));
                        identity.AddClaim(new Claim("email", email));
                    }

                    // traefikoidc aggregates groups/roles; support comma or semicolon separators
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
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }
}
