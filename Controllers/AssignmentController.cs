using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;
using MerkApi.Services;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AssignmentChecker _checker;

        public AssignmentController(AppDbContext context)
        {
            _context = context;
            _checker = new AssignmentChecker();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignments()
            => await _context.Assignments.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            return assignment;
        }

        [HttpPost]
        public async Task<ActionResult<Assignment>> CreateAssignment(CreateAssignmentRequest request)
        {
            var assignment = new Assignment
            {
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                Options = request.Options,          // <-- баг с исчезновением вариантов: теперь сохраняем
                CorrectAnswer = request.CorrectAnswer,
                TeacherId = request.TeacherId,
                GroupId = request.GroupId
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
        }

        [HttpPost("submit")]
        public async Task<ActionResult> SubmitAnswer(SubmitAnswerRequest request)
        {
            var assignment = await _context.Assignments.FindAsync(request.AssignmentId);
            if (assignment == null) return NotFound("Задание не найдено");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("Пользователь не найден");

            var (grade, maxGrade, comment) = _checker.CheckAnswer(assignment, request.StudentAnswer);

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

            return Ok(new SubmitAnswerResponse
            {
                Grade = grade,
                MaxGrade = maxGrade,
                Comment = comment
            });
        }

        [HttpGet("student/{userId}")]
        public async Task<ActionResult<IEnumerable<AssignmentStatusDto>>> GetAssignmentsForStudent(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Пользователь не найден");

            // Преподаватели, к которым привязан студент
            var teacherIds = await _context.TeacherStudents
                .Where(ts => ts.StudentId == userId)
                .Select(ts => ts.TeacherId)
                .ToListAsync();

            // Только задания этих преподавателей, видимые группе студента
            var assignments = await _context.Assignments
                .Where(a => teacherIds.Contains(a.TeacherId)
                            && (a.GroupId == null || a.GroupId == user.GroupId))
                .ToListAsync();

            var submissions = await _context.Submissions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var latest = submissions
                .GroupBy(s => s.AssignmentId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.SubmittedAt).First());

            var result = assignments.Select(a => new AssignmentStatusDto
            {
                AssignmentId = a.Id,
                Title = a.Title,
                Description = a.Description,
                Type = a.Type,
                Options = a.Options,
                IsCompleted = latest.ContainsKey(a.Id),
                Grade = latest.ContainsKey(a.Id) ? latest[a.Id].Grade : (int?)null,
                MaxGrade = latest.ContainsKey(a.Id) ? latest[a.Id].MaxGrade : (int?)null,
                Comment = latest.ContainsKey(a.Id) ? latest[a.Id].Comment : null
            }).ToList();

            return result;
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetTeacherAssignments(int teacherId)
            => await _context.Assignments.Where(a => a.TeacherId == teacherId).ToListAsync();

        [HttpPut("{id}")]
        public async Task<ActionResult<Assignment>> UpdateAssignment(int id, CreateAssignmentRequest request)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            assignment.Title = request.Title;
            assignment.Description = request.Description;
            assignment.Type = request.Type;
            assignment.Options = request.Options;
            assignment.CorrectAnswer = request.CorrectAnswer;
            assignment.GroupId = request.GroupId;

            await _context.SaveChangesAsync();
            return assignment;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
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
}