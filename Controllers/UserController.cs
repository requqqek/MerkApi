using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;

namespace MerkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile([FromQuery] int userId)
    {
        var user = await _context.Users
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        var profile = new UserProfileDto
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            GroupId = user.GroupId,
            GroupName = user.Group?.Name
        };

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest request, [FromQuery] int userId)
    {
        var user = await _context.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        if (request.Email != null)
            user.Email = request.Email;

        if (request.Phone != null)
            user.Phone = request.Phone;

        if (request.Password != null && request.Password.Length >= 6)
            user.Password = request.Password;

        await _context.SaveChangesAsync();

        var profile = new UserProfileDto
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            GroupId = user.GroupId,
            GroupName = user.Group?.Name
        };

        return Ok(profile);
    }

    [HttpPut("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });
        if (user.Password != request.OldPassword) return BadRequest(new { message = "Неверный старый пароль" });
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "Пароль минимум 6 символов" });

        user.Password = request.NewPassword;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Пароль изменён" });
    }
}
