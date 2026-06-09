using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MerkApi.Data;
using MerkApi.DTOs;
using MerkApi.Models;

namespace MerkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvitationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<InviteCodeDto>>> GetCodes(int teacherId)
        {
            var codes = await _context.InvitationCodes
                .Where(ic => ic.TeacherId == teacherId)
                .ToListAsync();
            var groups = await _context.Groups.ToListAsync();

            return codes.Select(ic => new InviteCodeDto
            {
                Id = ic.Id,
                Code = ic.Code,
                GroupId = ic.GroupId,
                GroupName = groups.FirstOrDefault(g => g.Id == ic.GroupId)?.Name,
                IsActive = ic.IsActive
            }).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<InviteCodeDto>> CreateCode(CreateInviteCodeRequest request)
        {
            string code = GenerateCode();
            while (await _context.InvitationCodes.AnyAsync(ic => ic.Code == code))
                code = GenerateCode();

            var invite = new InvitationCode
            {
                Code = code,
                TeacherId = request.TeacherId,
                GroupId = request.GroupId,
                IsActive = true
            };
            _context.InvitationCodes.Add(invite);
            await _context.SaveChangesAsync();

            string? groupName = request.GroupId == null
                ? null
                : (await _context.Groups.FindAsync(request.GroupId.Value))?.Name;

            return new InviteCodeDto
            {
                Id = invite.Id,
                Code = invite.Code,
                GroupId = invite.GroupId,
                GroupName = groupName,
                IsActive = invite.IsActive
            };
        }

        private static string GenerateCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = Random.Shared;
            return new string(Enumerable.Range(0, 6).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }
    }
}