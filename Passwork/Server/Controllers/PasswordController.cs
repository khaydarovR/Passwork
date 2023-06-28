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
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiHub _apiHub;

        public PasswordController(AppDbContext context, ApiHub apiHub)
        {
            this._context = context;
            _apiHub = apiHub;
        }


        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] PasswordCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var newPassword = new Password
            {
                Title = model.Title,
                Login = model.Login,
                Pw = model.Pw,
                Note = model.Note,
                SafeId = model.SafeId,
                UseInUtl = model.UseInUrl,
                IsDeleted = false,
            };
            await _context.Passwords.AddAsync(newPassword);
            _context.SaveChanges();

            var tags = new List<Tag>();
            foreach (var tag in model.Tags)
            {
                tags.Add(new Tag { Title = tag });
            }
            await _context.Tags.AddRangeAsync(tags);
            _context.SaveChanges();

            var pasTags = new List<PasswordTags>();
            foreach (var tag in tags)
            {
                pasTags.Add(new PasswordTags() { PasswordId = newPassword.Id, TagId = tag.Id });
            }
            await _context.PasswordTags.AddRangeAsync(pasTags);
            _context.SaveChanges();

            await _apiHub.SendPasswordUpdate(id);

            return Ok();
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<List<PasswordVm>>> Get([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }
            var result = new List<PasswordVm>();

            var passwords = await _context.Passwords
                .Include(p => p.PasswordTags)
                .ThenInclude(p => p.Tag)
                .Where(p => p.SafeId == safeId)
                .ToListAsync();

            foreach (var p in passwords)
            {
                result.Add(p.MapToVm());
            }

            return Ok(result);
        }
    }
}
