namespace MerkApi.DTOs
{
    public class AssignmentStatusDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Options { get; set; }
        public bool IsCompleted { get; set; }
        public int? Grade { get; set; }
        public int? MaxGrade { get; set; }
        public string? Comment { get; set; }
    }
}