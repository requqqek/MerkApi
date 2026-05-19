// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MerkApi.Models
{
    [Table("userinfo", Schema = "dbo")]
    public class User
    {
        [Key]
        [Column("nameUser")]
        [MaxLength(50)]
        public string NameUser { get; set; } = string.Empty;

        [Column("email")]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Column("mobNumber")]
        [MaxLength(15)]
        [Phone]
        public string MobNumber { get; set; } = string.Empty;

        [Column("password")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = "user";  // default: "user", может быть "admin"
    }
}