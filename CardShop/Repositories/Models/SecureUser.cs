using Microsoft.AspNetCore.Identity;

namespace CardShop.Repositories.Models
{
    public class SecureUser : IdentityUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public Enums.Role Role { get; set; }
    }
}
