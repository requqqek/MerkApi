// DTOs/UpdateUserDto.cs
namespace MerkApi.DTOs
{
    public class UpdateUserDto
    {
        public string? NameUser { get; set; }
        public string? Email { get; set; }
        public string? MobNumber { get; set; }
        public string? Password { get; set; }
    }
}