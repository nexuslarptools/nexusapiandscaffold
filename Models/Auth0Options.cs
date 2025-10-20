using System.ComponentModel.DataAnnotations;

namespace NEXUSDataLayerScaffold.Models
{
    public class Auth0Options
    {
        [Required]
        public string Domain { get; set; } = string.Empty; // e.g., your-tenant.auth0.com

        // Identifier of the API/audience to request and validate
        [Required]
        public string ApiIdentifier { get; set; } = string.Empty;

        // Client credentials for confidential client scenarios (optional when using PKCE-only)
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        // Redirect URIs for OIDC Authorization Code Flow (optional; may be inferred)
        public string? RedirectUri { get; set; }
        public string? PostLogoutRedirectUri { get; set; }

        // Optional: client authentication method (client_secret_post | client_secret_basic)
        public string? ClientAuthMethod { get; set; }
    }

    public class FrontendOptions
    {
        public string? DefaultReturnUrl { get; set; }
    }

    public class SecurityOptions
    {
        public string[]? AllowedRedirectHosts { get; set; }
    }
}
