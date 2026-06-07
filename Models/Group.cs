namespace MerkApi.Models;

/// <summary>
/// Группа студентов (например, "ИС-21", "ПИ-22").
/// </summary>
public class Group
{
    public int Id { get; set; }

    /// <summary>Название группы (например, "ИС-21").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Навигационное свойство: все студенты в этой группе.</summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}