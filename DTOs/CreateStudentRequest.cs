namespace MerkApi.DTOs
{
    public class CreateStudentRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        /// <summary>Преподаватель (при добавлении студента учителем).</summary>
        public int? TeacherId { get; set; }
        public int? GroupId { get; set; }

        /// <summary>Код приглашения (при самостоятельной регистрации).</summary>
        public string? InviteCode { get; set; }
    }
}