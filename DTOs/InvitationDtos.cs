namespace MerkApi.DTOs
{
    public class CreateInviteCodeRequest
    {
        public int TeacherId { get; set; }
        public int? GroupId { get; set; }
    }

    public class InviteCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; }
    }
}