using System.ComponentModel.DataAnnotations;

namespace NEXUSDataLayerScaffold.Models
{
    public class Auth0Options
    {
        [Required]
        public string Domain { get; set; } = string.Empty;

        [Required]
        public string ApiIdentifier { get; set; } = string.Empty;

        // Optional: Only needed for Management API or client credentials flows (not required for JWT validation)
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
