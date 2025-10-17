using System.ComponentModel.DataAnnotations;

namespace NEXUSDataLayerScaffold.Models
{
    public class Auth0Options
    {
        [Required]
        public string Domain { get; set; } = string.Empty;

        [Required]
        public string ApiIdentifier { get; set; } = string.Empty;
    }
}
