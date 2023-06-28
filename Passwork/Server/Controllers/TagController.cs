using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Utils;
using Passwork.Shared.ViewModels;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiHub _apiHub;

        public TagController(AppDbContext context, ApiHub apiHub)
        {
            this._context = context;
            _apiHub = apiHub;
        }


        [HttpGet("Get")]
        [Authorize]
        public async Task<ActionResult<List<TagVm>>> Get([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }
            var result = new List<TagVm>();

            var passwordIds = await _context.Passwords
                .Where(p => p.SafeId == safeId)
                .Select(p => p.Id)
                .ToListAsync();

            if (passwordIds.Count == 0)
            {
                return Ok(result);
            }

            var paswordTags = await _context.PasswordTags
                .Include(t => t.Password)
                .Include(t => t.Tag)
                .Where(pt => passwordIds.Contains(pt.PasswordId))
                .Select(t => t.TagId)
                .ToListAsync();

            var tags = await _context.Tags
                .Where(t => paswordTags.Contains(t.Id))
                .ToListAsync();

            foreach (var t in tags)
            {
                result.Add(t.MapToVm());
            }

            result = result.Distinct().ToList();

            return Ok(result);
        }
    }
}
