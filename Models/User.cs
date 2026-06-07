// Путь: MerkApi/Models/User.cs
namespace MerkApi.Models;

/// <summary>
/// Модель пользователя системы (студент или преподаватель).
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Логин для входа в систему.</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Пароль (в MVP хранится открыто; в продакшене — хэш bcrypt).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Роль пользователя: "Student" или "Teacher".</summary>
    public string Role { get; set; } = "Student";

    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>ID группы (только для студентов).</summary>
    public int? GroupId { get; set; }

    /// <summary>Навигационное свойство: группа студента.</summary>
    public Group? Group { get; set; }
}