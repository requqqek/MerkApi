// Путь: MerkApi/Data/DbSeeder.cs
using MerkApi.Models;

namespace MerkApi.Data;

/// <summary>
/// Заполняет базу данных тестовыми данными при первом запуске.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Добавляем группы, если их нет
        if (!db.Groups.Any())
        {
            db.Groups.AddRange(
                new Group { Name = "ИС-21" },
                new Group { Name = "ИС-22" },
                new Group { Name = "ПИ-21" }
            );
            db.SaveChanges();
        }

        // Добавляем преподавателя, если его нет
        if (!db.Users.Any(u => u.Login == "teacher"))
        {
            var teacher = new User
            {
                Login = "teacher",
                Password = "teacher123",
                Role = "Teacher",
                Email = "teacher@merk.ru"
            };
            db.Users.Add(teacher);
            db.SaveChanges();

            // Добавляем студентов
            var group1 = db.Groups.First(g => g.Name == "ИС-21");
            var group2 = db.Groups.First(g => g.Name == "ИС-22");

            var student1 = new User
            {
                Login = "student1",
                Password = "student123",
                Role = "Student",
                GroupId = group1.Id,
                Email = "student1@merk.ru"
            };

            var student2 = new User
            {
                Login = "student2",
                Password = "student123",
                Role = "Student",
                GroupId = group1.Id,
                Email = "student2@merk.ru"
            };

            var student3 = new User
            {
                Login = "student3",
                Password = "student123",
                Role = "Student",
                GroupId = group2.Id,
                Email = "student3@merk.ru"
            };

            db.Users.AddRange(student1, student2, student3);
            db.SaveChanges();

            // Привязываем студентов к преподавателю
            db.TeacherStudents.AddRange(
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student1.Id },
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student2.Id },
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student3.Id }
            );
            db.SaveChanges();
        }

        // Добавляем задания, если их нет
        if (!db.Assignments.Any())
        {
            db.Assignments.AddRange(
                new Assignment
                {
                    Title = "Выбор одного ответа",
                    Description = "Какой язык программирования используется для Android-разработки?",
                    Type = "SingleChoice",
                    Options = "[\"Python\", \"Kotlin\", \"JavaScript\", \"C++\"]",
                    CorrectAnswer = "1"
                },
                new Assignment
                {
                    Title = "Выбор нескольких ответов",
                    Description = "Какие из этих языков являются объектно-ориентированными?",
                    Type = "MultipleChoice",
                    Options = "[\"Java\", \"C\", \"Python\", \"Assembly\"]",
                    CorrectAnswer = "0,2"
                },
                new Assignment
                {
                    Title = "Текстовый ответ",
                    Description = "Какой оператор используется для вывода текста в Kotlin?",
                    Type = "TextInput",
                    CorrectAnswer = "println"
                },
                new Assignment
                {
                    Title = "Программный код",
                    Description = "Напишите программу, которая выводит 'Hello World'",
                    Type = "Code",
                    CorrectAnswer = "Hello World"
                }
            );
            db.SaveChanges();
        }
    }
}