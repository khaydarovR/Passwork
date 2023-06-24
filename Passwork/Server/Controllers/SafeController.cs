using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.Security.Claims;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SafeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SafeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }


        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] SafeCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }
            var id = User.Claims.First(c => c.Type == ClaimTypes.IsPersistent).Value;
            var company = _context.Companies.Single(c => c.Id == model.CompanyId);
            var owner = await _userManager.FindByIdAsync(id);

            var setId = Guid.NewGuid();
            await _context.Safes.AddAsync(new Safe
            {
                Id = setId,
                Title = model.Title,
                Company = company,
                Description = model.Description,
            });

            var newSafe = await _context.Safes.SingleAsync(s => s.Id ==  setId);
            var safeUser = new List<SafeUsers>() { new SafeUsers()
            {
                AppUser = owner,
                Right = Domain.RightEnum.Owner,
                Safe = newSafe
            } };

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
