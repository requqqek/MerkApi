using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;
using MerkApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MerkApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssignmentController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить все задания.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignments()
    {
        return await _context.Assignments.ToListAsync();
    }

    /// <summary>
    /// Получить задание по ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Assignment>> GetAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }
        return assignment;
    }

    /// <summary>
    /// Создать новое задание (только для преподавателей).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Assignment>> CreateAssignment(CreateAssignmentRequest request)
    {
        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            CorrectAnswer = request.CorrectAnswer
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
    }

    /// <summary>
    /// Отправить ответ на задание.
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult> SubmitAnswer(SubmitAnswerRequest request)
    {
        var assignment = await _context.Assignments.FindAsync(request.AssignmentId);
        if (assignment == null) return NotFound(new { message = "Задание не найдено" });

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null) return NotFound(new { message = "Пользователь не найден" });

        var checker = new MerkApi.Services.AssignmentChecker();
        var (grade, maxGrade, comment) = checker.CheckAnswer(assignment, request.StudentAnswer);

        var submission = new Submission
        {
            UserId = request.UserId,
            AssignmentId = request.AssignmentId,
            StudentAnswer = request.StudentAnswer,
            Grade = grade,
            MaxGrade = maxGrade,
            Comment = comment,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return Ok(new { grade, maxGrade, comment });
    }

    [HttpGet("student/{userId}")]
    public async Task<ActionResult<IEnumerable<AssignmentStatusDto>>> GetAssignmentsForStudent(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Пользователь не найден" });
        }

        // Получаем все задания
        var assignments = await _context.Assignments.ToListAsync();

        // Получаем все попытки студента
        var submissions = await _context.Submissions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        // Создаём словарь: AssignmentId -> последняя попытка
        var latestSubmissions = submissions
            .GroupBy(s => s.AssignmentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.SubmittedAt).First());

        // Формируем результат
        var result = assignments.Select(a => new {
            AssignmentId = a.Id,
            Title = a.Title,
            Description = a.Description,
            Type = a.Type,
            Options = a.Options,
            IsCompleted = latestSubmissions.ContainsKey(a.Id),
            Grade = latestSubmissions.ContainsKey(a.Id) ? latestSubmissions[a.Id].Grade : (int?)null,
            MaxGrade = latestSubmissions.ContainsKey(a.Id) ? latestSubmissions[a.Id].MaxGrade : (int?)null,
            Comment = latestSubmissions.ContainsKey(a.Id) ? latestSubmissions[a.Id].Comment : null
        }).ToList();

        return Ok(result);
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetTeacherAssignments(int teacherId)
    {
        // В MVP все задания видны всем преподавателям
        // В продакшене можно добавить фильтрацию по автору
        return await _context.Assignments.ToListAsync();
    }

    /// <summary>
    /// Обновить задание.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Assignment>> UpdateAssignment(int id, CreateAssignmentRequest request)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound(new { message = "Задание не найдено" });
        }

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.Type = request.Type;
        assignment.Options = request.Options;
        assignment.CorrectAnswer = request.CorrectAnswer;

        await _context.SaveChangesAsync();

        return Ok(assignment);
    }

    /// <summary>
    /// Удалить задание.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound(new { message = "Задание не найдено" });
        }

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Задание удалено" });
    }

    [HttpGet("stats/{userId}")]
    public async Task<ActionResult> GetStudentStats(int userId)
    {
        var submissions = await _context.Submissions.Where(s => s.UserId == userId).ToListAsync();
        var totalAttempts = submissions.Count;
        var totalGrade = submissions.Sum(s => s.Grade);
        var totalMax = submissions.Sum(s => s.MaxGrade);
        var avgPercent = totalMax > 0 ? Math.Round((double)totalGrade / totalMax * 100, 1) : 0.0;

        return Ok(new { totalAttempts, totalGrade, totalMax, averagePercent = avgPercent });
    }
}