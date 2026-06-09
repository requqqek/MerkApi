using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Services;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly CryptoService _cryptoService;

        public UserController(AppDbContext context, PasswordService passwordService, CryptoService cryptoService)
        {
            _context = context;
            _passwordService = passwordService;
            _cryptoService = cryptoService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile([FromQuery] int userId)
        {
            var user = await _context.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return new UserProfileDto
            {
                UserId = user.Id,
                Login = user.Login,
                Email = _cryptoService.Decrypt(user.Email),
                Phone = _cryptoService.Decrypt(user.Phone),
                Role = user.Role,
                GroupId = user.GroupId,
                GroupName = user.Group?.Name
            };
        }

        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest request, [FromQuery] int userId)
        {
            var user = await _context.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (request.Email != null) user.Email = _cryptoService.Encrypt(request.Email);
            if (request.Phone != null) user.Phone = _cryptoService.Encrypt(request.Phone);
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password.Length < 6) return BadRequest("Пароль должен содержать минимум 6 символов");
                user.Password = _passwordService.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();

            return new UserProfileDto
            {
                UserId = user.Id,
                Login = user.Login,
                Email = _cryptoService.Decrypt(user.Email),
                Phone = _cryptoService.Decrypt(user.Phone),
                Role = user.Role,
                GroupId = user.GroupId,
                GroupName = user.Group?.Name
            };
        }

        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound();

            if (!_passwordService.VerifyPassword(request.OldPassword, user.Password))
                return BadRequest("Старый пароль неверный");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest("Новый пароль должен содержать минимум 6 символов");

            user.Password = _passwordService.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}