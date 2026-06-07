namespace MerkApi.DTOs;

public class SubmitAnswerRequest
{
    public int UserId { get; set; }
    public int AssignmentId { get; set; }
    public string StudentAnswer { get; set; } = string.Empty;
}