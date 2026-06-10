using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Services;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CryptoService _cryptoService;

        public TeacherController(AppDbContext context, CryptoService cryptoService)
        {
            _context = context;
            _cryptoService = cryptoService;
        }

        [HttpGet("submissions")]
        public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetSubmissions(
            [FromQuery] int? studentId = null,
            [FromQuery] int? groupId = null,
            [FromQuery] int? teacherId = null)
        {
            var query = _context.Submissions
                .Include(s => s.User)
                    .ThenInclude(u => u!.Group)
                .Include(s => s.Assignment)
                .AsQueryable();

            if (studentId.HasValue)
                query = query.Where(s => s.UserId == studentId.Value);

            if (groupId.HasValue)
                query = query.Where(s => s.User!.GroupId == groupId.Value);

            if (teacherId.HasValue)
            {
                var studentIds = await _context.TeacherStudents
                    .Where(ts => ts.TeacherId == teacherId.Value)
                    .Select(ts => ts.StudentId)
                    .ToListAsync();
                query = query.Where(s => studentIds.Contains(s.UserId));
            }

            var result = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new SubmissionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    AssignmentId = s.AssignmentId,
                    StudentAnswer = s.StudentAnswer,
                    Grade = s.Grade,
                    MaxGrade = s.MaxGrade,
                    Comment = s.Comment,
                    StudentLogin = s.User!.Login,
                    AssignmentTitle = s.Assignment!.Title,
                    SubmittedAt = s.SubmittedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync();

            return result;
        }

        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetTeacherStudents([FromQuery] int? teacherId = null)
        {
            var query = _context.TeacherStudents
                .Include(ts => ts.Student)
                    .ThenInclude(u => u!.Group)
                .AsQueryable();

            if (teacherId.HasValue)
                query = query.Where(ts => ts.TeacherId == teacherId.Value);

            var list = await query.ToListAsync();

            // Расшифровываем ПДн для отображения преподавателю
            var result = list.Select(ts => new StudentDto
            {
                UserId = ts.Student!.Id,
                Login = ts.Student.Login,
                Email = _cryptoService.Decrypt(ts.Student.Email),
                Phone = _cryptoService.Decrypt(ts.Student.Phone),
                GroupId = ts.Student.GroupId,
                GroupName = ts.Student.Group?.Name
            }).ToList();

            return result;
        }

        [HttpDelete("students/{studentId}")]
            public async Task<ActionResult> DeleteStudent(int studentId)
            {
                var user = await _context.Users.FindAsync(studentId);
                if (user == null) return NotFound("Студент не найден");
                if (user.Role != "Student") return BadRequest("Можно удалять только студентов");

                _context.Users.Remove(user); // каскадом удалятся его попытки и привязка к учителю
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
}