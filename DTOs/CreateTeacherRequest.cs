namespace MerkApi.DTOs
{
    public class CreateTeacherRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string AdminKey { get; set; } = string.Empty;
    }
}