namespace MerkApi.DTOs;

public class SubmitAnswerResponse
{
    public int Grade { get; set; }
    public int MaxGrade { get; set; }
    public string Comment { get; set; } = string.Empty;
}