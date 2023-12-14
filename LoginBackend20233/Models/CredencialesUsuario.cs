// CredencialesUsuario.cs

using System.ComponentModel.DataAnnotations;

namespace LoginBackend20233.Models
{
    public class CredencialesUsuario
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
