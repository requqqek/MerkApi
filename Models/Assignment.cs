namespace MerkApi.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>"SingleChoice" | "MultipleChoice" | "TextInput" | "Code"</summary>
        public string Type { get; set; } = "SingleChoice";

        /// <summary>Варианты ответов — JSON-массив строк.</summary>
        public string? Options { get; set; }

        /// <summary>Правильный ответ (индексы 0-based для choice; текст/вывод для остального).</summary>
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>Преподаватель-автор.</summary>
        public int TeacherId { get; set; }

        /// <summary>Группа, которой видно задание. null = всем студентам преподавателя.</summary>
        public int? GroupId { get; set; }
    }
}