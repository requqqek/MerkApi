// Controllers/AuthController.cs
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
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, PasswordService passwordService, JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Проверяем, существует ли имя пользователя
            if (await _context.Users.AnyAsync(u => u.NameUser == dto.NameUser))
            {
                return BadRequest(new { message = "Пользователь с таким именем уже существует" });
            }

            // Проверяем email
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            // Проверяем телефон
            if (await _context.Users.AnyAsync(u => u.MobNumber == dto.MobNumber))
            {
                return BadRequest(new { message = "Пользователь с таким телефоном уже существует" });
            }

            // Создаём пользователя
            var user = new User
            {
                NameUser = dto.NameUser,
                Email = dto.Email,
                MobNumber = dto.MobNumber,
                Password = _passwordService.HashPassword(dto.Password),
                Role = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Генерируем токен
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Регистрация успешна",
                token = token,
                user = new
                {
                    user.NameUser,
                    user.Email,
                    user.MobNumber,
                    user.Role
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Ищем пользователя по email ИЛИ телефону
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.EmailOrPhone
                                       || u.MobNumber == dto.EmailOrPhone);

            if (user == null)
            {
                return Unauthorized(new { message = "Неверный email/телефон или пароль" });
            }

            // Проверяем пароль
            if (!_passwordService.VerifyPassword(dto.Password, user.Password))
            {
                return Unauthorized(new { message = "Неверный email/телефон или пароль" });
            }

            // Генерируем токен
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Вход выполнен",
                token = token,
                user = new
                {
                    user.NameUser,
                    user.Email,
                    user.MobNumber,
                    user.Role
                }
            });
        }

        [HttpGet("{nameUser}")]
        public async Task<IActionResult> GetUser(string nameUser)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NameUser == nameUser);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            return Ok(new
            {
                user.NameUser,
                user.Email,
                user.MobNumber,
                user.Role
            });
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto dto)
        {
            // Проверяем, существует ли уже администратор
            var adminExists = await _context.Users.AnyAsync(u => u.Role == "admin");
            if (adminExists)
            {
                return BadRequest(new { message = "Администратор уже существует" });
            }

            // Проверяем, существует ли имя пользователя
            if (await _context.Users.AnyAsync(u => u.NameUser == dto.NameUser))
            {
                return BadRequest(new { message = "Пользователь с таким именем уже существует" });
            }

            // Проверяем email
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            // Проверяем телефон
            if (await _context.Users.AnyAsync(u => u.MobNumber == dto.MobNumber))
            {
                return BadRequest(new { message = "Пользователь с таким телефоном уже существует" });
            }

            // Создаём администратора
            var admin = new User
            {
                NameUser = dto.NameUser,
                Email = dto.Email,
                MobNumber = dto.MobNumber,
                Password = _passwordService.HashPassword(dto.Password),
                Role = "admin"
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Администратор успешно создан" });
        }

        [HttpPut("{nameUser}")]
        public async Task<IActionResult> UpdateUser(string nameUser, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NameUser == nameUser);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            // Обновляем email
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email && u.NameUser != nameUser);
                if (emailExists)
                {
                    return BadRequest(new { message = "Этот email уже используется" });
                }
                user.Email = dto.Email;
            }

            // Обновляем телефон
            if (!string.IsNullOrEmpty(dto.MobNumber))
            {
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.MobNumber == dto.MobNumber && u.NameUser != nameUser);
                if (phoneExists)
                {
                    return BadRequest(new { message = "Этот телефон уже используется" });
                }
                user.MobNumber = dto.MobNumber;
            }

            // Обновляем пароль
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password.Length < 6)
                {
                    return BadRequest(new { message = "Пароль должен быть минимум 6 символов" });
                }
                user.Password = _passwordService.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Данные успешно обновлены" });
        }

        [HttpDelete("{nameUser}")]
        public async Task<IActionResult> DeleteUser(string nameUser)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.NameUser == nameUser);

            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь удалён" });
        }

        [HttpGet("all/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.NameUser,
                    u.Email,
                    u.MobNumber,
                    u.Role
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}