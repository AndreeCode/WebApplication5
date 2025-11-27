using System.ComponentModel.DataAnnotations;

namespace WebApplication5.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Location { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
