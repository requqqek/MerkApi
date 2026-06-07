// Путь: MerkApi/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;

namespace MerkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Вход в систему.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Login == request.Login && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Неверный логин или пароль" });
        }

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Role = user.Role
        });
    }

    /// <summary>
    /// Создать нового студента.
    /// </summary>
    [HttpPost("create-student")]
    public async Task<ActionResult> CreateStudent(CreateStudentRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Логин уже занят" });
        }

        var user = new User
        {
            Login = request.Login,
            Password = request.Password,
            Email = request.Email,
            Phone = request.Phone,
            Role = "Student",
            GroupId = request.GroupId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Студент успешно создан", userId = user.Id });
    }
}