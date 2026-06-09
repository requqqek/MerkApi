using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/Groups")]   // важно: приложение обращается к /api/Groups
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
            => await _context.Groups.OrderBy(g => g.Name).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();
            return group;
        }

        // НОВОЕ: создание группы преподавателем
        [HttpPost]
        public async Task<ActionResult<Group>> CreateGroup(CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Введите название группы");

            var name = request.Name.Trim();
            if (await _context.Groups.AnyAsync(g => g.Name == name))
                return BadRequest("Группа с таким названием уже существует");

            var group = new Group { Name = name };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(group);
        }
    }
}