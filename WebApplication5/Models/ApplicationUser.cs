using Microsoft.AspNetCore.Identity;

namespace WebApplication5.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Location { get; set; }

        // Navigation
        public ICollection<Service>? Services { get; set; }
    }
}
