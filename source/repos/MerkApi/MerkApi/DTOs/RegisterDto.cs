// DTOs/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace MerkApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string NameUser { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(15)]
        public string MobNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}