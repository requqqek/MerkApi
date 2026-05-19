// DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace MerkApi.DTOs
{
    public class LoginDto
    {
        [Required]
        public string EmailOrPhone { get; set; } = string.Empty;  // ← убедитесь, что поле существует

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}