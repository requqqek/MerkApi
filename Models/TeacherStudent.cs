namespace MerkApi.Models;

/// <summary>
/// Связь "преподаватель — студент". Определяет, каких студентов видит преподаватель.
/// </summary>
public class TeacherStudent
{
    public int Id { get; set; }

    /// <summary>ID преподавателя.</summary>
    public int TeacherId { get; set; }

    /// <summary>ID студента.</summary>
    public int StudentId { get; set; }

    /// <summary>Навигационное свойство: преподаватель.</summary>
    public User? Teacher { get; set; }

    /// <summary>Навигационное свойство: студент.</summary>
    public User? Student { get; set; }
}