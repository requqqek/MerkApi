namespace MerkApi.Models
{
    /// <summary>
    /// Код приглашения. Студент вводит его при регистрации и автоматически
    /// закрепляется за преподавателем (и группой).
    /// </summary>
    public class InvitationCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public int? GroupId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}