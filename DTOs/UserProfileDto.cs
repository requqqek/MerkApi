namespace MerkApi.DTOs
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
    }
}