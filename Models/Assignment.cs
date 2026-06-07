namespace MerkApi.Models;

public class Assignment
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип задания:
    /// - "SingleChoice" - выбор одного ответа
    /// - "MultipleChoice" - выбор нескольких ответов
    /// - "TextInput" - текстовый ввод
    /// - "Code" - программный код
    /// </summary>
    public string Type { get; set; } = "SingleChoice";

    /// <summary>
    /// Варианты ответов (для SingleChoice и MultipleChoice).
    /// Формат: JSON массив строк, например: ["Ответ 1", "Ответ 2", "Ответ 3"]
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Правильный ответ (или ответы).
    /// Для SingleChoice: индекс правильного ответа (0, 1, 2...)
    /// Для MultipleChoice: индексы через запятую (0,2,3)
    /// Для TextInput: точный текст ответа
    /// Для Code: ожидаемый вывод программы
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;
}