using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
