using System.ComponentModel.DataAnnotations;

namespace NEXUSDataLayerScaffold.Models
{
    public class OAuthOptions
    {
        // Generic OIDC/OAuth authority (e.g., https://accounts.example.com or https://login.microsoftonline.com/{tenant}/v2.0)
        [Required]
        public string Authority { get; set; } = string.Empty;

        // Audience (API identifier) expected in access tokens, if applicable
        public string? Audience { get; set; }

        // Optional claim type mappings
        public string? RoleClaimType { get; set; }
        public string? NameClaimType { get; set; }
    }
}
