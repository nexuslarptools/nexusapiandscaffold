### Authentication cookies used by this application

This service authenticates requests primarily via the ASP.NET Core Cookie Authentication handler (plus JWT Bearer tokens). In the current BFF/reverse‑proxy setup, the proxy handles the front‑channel OIDC flow, and this API validates requests using either the Authorization header (Bearer) or the auth cookie.

Below are the cookies relevant to authentication in this service.

- Primary auth cookie
  - Name: _oidc_raczylo
  - Purpose: Holds the encrypted authentication ticket (ClaimsPrincipal and AuthenticationProperties) issued by the Cookie authentication handler. When tokens are saved by an OIDC flow, their metadata (e.g., access_token/id_token/refresh_token presence and expiry) is stored within the encrypted ticket, not as separate cookies.
  - Attributes configured:
    - HttpOnly: true (framework default)
    - Secure: Always (Startup config)
    - SameSite: None (Startup config)
    - Path: "/" (framework default)

- Correlation/nonce cookies (only if the app itself initiates an OIDC challenge)
  - Names:
    - .AspNetCore.Correlation.OpenIdConnect
    - .AspNetCore.OpenIdConnect.Nonce
  - Purpose: Used by the OpenIdConnect handler to protect the authorization request/response. In the current BFF mode these are typically not emitted by this API because the reverse proxy/middleware owns the front‑channel OIDC flow. They may appear if you explicitly challenge with the OpenIdConnect scheme from this app.
  
- Other cookies
  - The application does not enable ASP.NET Core Session, antiforgery, or Identity UI; therefore, no session or antiforgery cookies are created by this API.

How cookies are used in the request pipeline
- Default schemes (Startup.cs): A policy scheme named "Smart" chooses between Bearer (when an Authorization header is present) and Cookies (otherwise) for authentication. Challenges return 401 (JwtBearer), avoiding OIDC redirects in BFF mode.
- No header injection: This API no longer injects an Authorization header from a cookie session. Requests are authenticated either by the Bearer token or the auth cookie (ClaimsPrincipal) directly. Controllers and authorization rely solely on the authenticated `HttpContext.User`.

Where this is configured in code
- Startup.cs
  - services.AddAuthentication(...) with DefaultAuthenticateScheme = "Smart" and DefaultChallengeScheme = JwtBearer
  - .AddCookie(...) with:
    - options.Cookie.SameSite = SameSiteMode.None
    - options.Cookie.SecurePolicy = CookieSecurePolicy.Always
  - services.ConfigureApplicationCookie(...) setting the same cookie security attributes

Notes
- Cookie name: The cookie authentication handler is configured to use the name "_oidc_raczylo" (and the application cookie is mirrored as "_oidc_raczylo_id_0") to align with OIDC cookie naming conventions.
- Token storage: Access/ID/refresh tokens (when present) are stored inside the encrypted authentication ticket, not as individual cookies. The Auth/Session endpoint (if implemented upstream) should reveal only token presence/expiry, never token values.
- Reverse proxy: With an upstream OIDC middleware (e.g., Traefik), the API typically receives either a Bearer token or an already-established cookie session. The API itself does not perform the front-channel OIDC redirects in this mode.