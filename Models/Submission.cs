namespace MerkApi.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AssignmentId { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int MaxGrade { get; set; }
        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Assignment? Assignment { get; set; }
    }
}