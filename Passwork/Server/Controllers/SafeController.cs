using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.Security.Claims;


namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SafeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiHub _apiHub;

        public SafeController(AppDbContext context, ApiHub apiHub)
        {
            this._context = context;
            _apiHub = apiHub;
        }


        [HttpPost("Create")]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] SafeCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value;

            var newSafe = new Safe
            {
                Title = model.Title,
                CompanyId = model.CompanyId,
                Description = model.Description,
            };
            await _context.Safes.AddAsync(newSafe);
            _context.SaveChanges();

            var safeUser = new SafeUsers()
            {
                AppUserId = Guid.Parse(id),
                Right = RightEnum.Owner,
                SafeId = newSafe.Id
            };
            await _context.SafeUsers.AddAsync(safeUser);

            await _context.SaveChangesAsync();
            await _apiHub.SendCompanyUpdate(id);
            return Ok();
        }


        [HttpGet("Users")]
        [Authorize]
        public async Task<ActionResult<List<SafeUserVm>>> Users([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }

            var result = new List<SafeUserVm>();

            var safeUsers = await _context.SafeUsers
                .Include(s => s.AppUser)
                .Where(s => s.SafeId == safeId)
                .ToListAsync();

            foreach (var su in safeUsers)
            {
                result.Add(su.MapToVm());
            }

            return Ok(result);
        }
    }
}
