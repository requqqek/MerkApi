namespace MerkApi.Models
{
    /// <summary>
    /// Пользователь системы (студент или преподаватель).
    /// Password — BCrypt-хеш. Email/Phone — зашифрованы AES (152-ФЗ).
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Student";
        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        /// <summary>Подряд неудачных попыток входа.</summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>Блокировка входа до указанного времени (null = не заблокирован).</summary>
        public DateTime? LockoutUntil { get; set; }
    }
}