// Путь: MerkApi/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MerkApi.Models;

namespace MerkApi.Data;

/// <summary>
/// Контекст базы данных EF Core. Отображает модели на таблицы SQLite.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<TeacherStudent> TeacherStudents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка связи User -> Group
        modelBuilder.Entity<User>()
            .HasOne(u => u.Group)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Настройка связи TeacherStudent
        modelBuilder.Entity<TeacherStudent>()
            .HasOne(ts => ts.Teacher)
            .WithMany()
            .HasForeignKey(ts => ts.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeacherStudent>()
            .HasOne(ts => ts.Student)
            .WithMany()
            .HasForeignKey(ts => ts.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Уникальный индекс для TeacherStudent
        modelBuilder.Entity<TeacherStudent>()
            .HasIndex(ts => new { ts.TeacherId, ts.StudentId })
            .IsUnique();
    }
}