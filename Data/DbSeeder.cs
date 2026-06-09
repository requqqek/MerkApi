using MerkApi.Models;
using MerkApi.Services;

namespace MerkApi.Data
{
    /// <summary>Заполняет БД тестовыми данными (пароли — хеш, ПДн — шифр).</summary>
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db, PasswordService passwords, CryptoService crypto)
        {
            // Группы: ИСП-100..106, ИСП-200..206, ИСП-300..306, ИСП-400..406
            if (!db.Groups.Any())
            {
                var names = new List<string>();
                foreach (var course in new[] { 100, 200, 300, 400 })
                    for (int n = 0; n <= 6; n++)
                        names.Add($"ИСП-{course + n}");

                db.Groups.AddRange(names.Select(name => new Group { Name = name }));
                db.SaveChanges();
            }

            if (db.Users.Any(u => u.Login == "teacher")) return;

            var teacher = new User
            {
                Login = "teacher",
                Password = passwords.HashPassword("teacher123"),
                Email = crypto.Encrypt("teacher@merk.ru"),
                Phone = crypto.Encrypt("+70000000000"),
                Role = "Teacher"
            };
            db.Users.Add(teacher);
            db.SaveChanges();

            var group1 = db.Groups.First(g => g.Name == "ИСП-100");
            var group2 = db.Groups.First(g => g.Name == "ИСП-101");

            var student1 = new User
            {
                Login = "student1",
                Password = passwords.HashPassword("student123"),
                Email = crypto.Encrypt("student1@merk.ru"),
                Phone = crypto.Encrypt("+70000000001"),
                Role = "Student",
                GroupId = group1.Id
            };
            var student2 = new User
            {
                Login = "student2",
                Password = passwords.HashPassword("student123"),
                Email = crypto.Encrypt("student2@merk.ru"),
                Phone = crypto.Encrypt("+70000000002"),
                Role = "Student",
                GroupId = group1.Id
            };
            var student3 = new User
            {
                Login = "student3",
                Password = passwords.HashPassword("student123"),
                Email = crypto.Encrypt("student3@merk.ru"),
                Phone = crypto.Encrypt("+70000000003"),
                Role = "Student",
                GroupId = group2.Id
            };
            db.Users.AddRange(student1, student2, student3);
            db.SaveChanges();

            db.TeacherStudents.AddRange(
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student1.Id },
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student2.Id },
                new TeacherStudent { TeacherId = teacher.Id, StudentId = student3.Id });
            db.SaveChanges();

            db.Assignments.AddRange(
                new Assignment
                {
                    Title = "Выбор языка",
                    Description = "Какой язык используется для Android?",
                    Type = "SingleChoice",
                    Options = "[\"Python\",\"Kotlin\",\"PHP\",\"Ruby\"]",
                    CorrectAnswer = "1",          // 0-based: Kotlin
                    TeacherId = teacher.Id,
                    GroupId = null                // видно всем студентам преподавателя
                },
                new Assignment
                {
                    Title = "Принципы ООП",
                    Description = "Выберите принципы ООП",
                    Type = "MultipleChoice",
                    Options = "[\"Инкапсуляция\",\"Наследование\",\"Рекурсия\",\"Полиморфизм\"]",
                    CorrectAnswer = "0,1,3",
                    TeacherId = teacher.Id,
                    GroupId = group1.Id           // только ИСП-100
                },
                new Assignment
                {
                    Title = "Сумма чисел",
                    Description = "Чему равно 2 + 2?",
                    Type = "TextInput",
                    Options = null,
                    CorrectAnswer = "4",
                    TeacherId = teacher.Id,
                    GroupId = null
                });
            db.SaveChanges();

            db.InvitationCodes.AddRange(
                new InvitationCode { Code = "ISP100", TeacherId = teacher.Id, GroupId = group1.Id, IsActive = true },
                new InvitationCode { Code = "ISP101", TeacherId = teacher.Id, GroupId = group2.Id, IsActive = true });
            db.SaveChanges();
        }
    }
}