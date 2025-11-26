using System.ComponentModel.DataAnnotations;

namespace WebApplication5.ViewModels
{
    public class ManageViewModel
    {
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Location { get; set; }
    }
}
