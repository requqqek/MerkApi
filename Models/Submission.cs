namespace MerkApi.Models;

public class Submission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }
    public string StudentAnswer { get; set; } = string.Empty;
    public int Grade { get; set; }
    public int MaxGrade { get; set; }  
    public string Comment { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}