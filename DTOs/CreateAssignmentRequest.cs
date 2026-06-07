namespace MerkApi.DTOs;

public class CreateAssignmentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Test";
    public string? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
}