using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;
using MerkApi.Services;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly CryptoService _cryptoService;

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AuthController(AppDbContext context, PasswordService passwordService, CryptoService cryptoService)
        {
            _context = context;
            _passwordService = passwordService;
            _cryptoService = cryptoService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (user == null)
                return Unauthorized("Неверный логин или пароль");

            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                var minutes = Math.Ceiling((user.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes);
                return StatusCode(423, $"Аккаунт временно заблокирован. Повторите через {minutes} мин.");
            }

            if (!_passwordService.VerifyPassword(request.Password, user.Password))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
                    user.FailedLoginAttempts = 0;
                    await _context.SaveChangesAsync();
                    return StatusCode(423, $"Превышено число попыток. Аккаунт заблокирован на {LockoutDuration.TotalMinutes} мин.");
                }
                await _context.SaveChangesAsync();
                var left = MaxFailedAttempts - user.FailedLoginAttempts;
                return Unauthorized($"Неверный логин или пароль. Осталось попыток: {left}");
            }

            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                UserId = user.Id,
                Login = user.Login,
                Role = user.Role
            });
        }

        [HttpPost("create-student")]
        public async Task<ActionResult> CreateStudent(CreateStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                return BadRequest("Введите логин");

            // Валидация email (если указан)
            if (!IsValidEmail(request.Email))
                return BadRequest("Некорректный email. Пример: name@mail.ru");

            // Валидация телефона (если указан)
            if (!IsValidPhone(request.Phone))
                return BadRequest("Некорректный номер телефона");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (existingUser != null)
                return BadRequest("Пользователь с таким логином уже существует");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return BadRequest("Пароль должен содержать минимум 6 символов");

            int? teacherId = request.TeacherId;
            int? groupId = request.GroupId;

            if (teacherId == null && !string.IsNullOrWhiteSpace(request.InviteCode))
            {
                var invite = await _context.InvitationCodes
                    .FirstOrDefaultAsync(ic => ic.Code == request.InviteCode && ic.IsActive);
                if (invite == null)
                    return BadRequest("Неверный или неактивный код приглашения");

                teacherId = invite.TeacherId;
                groupId ??= invite.GroupId;
            }

            var user = new User
            {
                Login = request.Login,
                Password = _passwordService.HashPassword(request.Password),
                Email = _cryptoService.Encrypt(request.Email),
                Phone = _cryptoService.Encrypt(request.Phone),
                Role = "Student",
                GroupId = groupId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (teacherId != null)
            {
                bool exists = await _context.TeacherStudents
                    .AnyAsync(ts => ts.TeacherId == teacherId && ts.StudentId == user.Id);
                if (!exists)
                {
                    _context.TeacherStudents.Add(new TeacherStudent
                    {
                        TeacherId = teacherId.Value,
                        StudentId = user.Id
                    });
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { userId = user.Id, login = user.Login });
        }

        // Email необязателен; если указан — должен быть корректным
        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains('@') && email.Contains('.');
            }
            catch
            {
                return false;
            }
        }

        // Телефон необязателен; если указан — минимум цифр
        private static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true;
            var digits = phone.Count(char.IsDigit);
            return digits is >= 10 and <= 15;
        }
    }
}