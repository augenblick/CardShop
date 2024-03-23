using System.ComponentModel.DataAnnotations;
using System.Data;

namespace CardShop.Models.Request
{
    public class RegistrationRequest
    {

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        // public Role Role { get; set; }

    }
}
