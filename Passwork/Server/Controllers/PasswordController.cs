using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.Security.Claims;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [Authorize]
        public async Task<ActionResult> Create([FromBody] PasswordCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value;

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
            
            return Ok();
        }
    }
}
