namespace MerkApi.DTOs;

public class UpdateProfileRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
}