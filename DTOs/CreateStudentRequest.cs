namespace MerkApi.DTOs;

public class CreateStudentRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? GroupId { get; set; }
}