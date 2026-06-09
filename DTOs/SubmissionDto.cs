namespace MerkApi.DTOs
{
    public class SubmissionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AssignmentId { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int MaxGrade { get; set; }
        public string? Comment { get; set; }
        public string? StudentLogin { get; set; }
        public string? AssignmentTitle { get; set; }
        public string? SubmittedAt { get; set; }
    }
}