// Путь: MerkApi/Controllers/TeacherController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;

namespace MerkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeacherController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить все попытки сдачи заданий с фильтрацией.
    /// </summary>
    [HttpGet("submissions")]
    public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetSubmissions(
        [FromQuery] int? studentId = null,
        [FromQuery] int? groupId = null)
    {
        var query = _context.Submissions
            .Include(s => s.User)
            .ThenInclude(u => u.Group)
            .Include(s => s.Assignment)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(s => s.UserId == studentId.Value);
        }

        if (groupId.HasValue)
        {
            query = query.Where(s => s.User.GroupId == groupId.Value);
        }

        var submissions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                UserId = s.UserId,
                AssignmentId = s.AssignmentId,
                StudentAnswer = s.StudentAnswer,
                Grade = s.Grade,
                Comment = s.Comment,
                SubmittedAt = s.SubmittedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                StudentLogin = s.User.Login,
                AssignmentTitle = s.Assignment.Title
            })
            .ToListAsync();

        return Ok(submissions);
    }

    /// <summary>
    /// Получить список студентов, привязанных к преподавателям.
    /// </summary>
    [HttpGet("students")]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetTeacherStudents()
    {
        var students = await _context.TeacherStudents
            .Include(ts => ts.Student)
            .ThenInclude(u => u.Group)
            .Select(ts => new StudentDto
            {
                UserId = ts.Student.Id,
                Login = ts.Student.Login,
                Email = ts.Student.Email,
                Phone = ts.Student.Phone,
                GroupId = ts.Student.GroupId,
                GroupName = ts.Student.Group != null ? ts.Student.Group.Name : null
            })
            .ToListAsync();

        return Ok(students);
    }
}