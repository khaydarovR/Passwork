using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.Security.Claims;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyController(AppDbContext context)
        {
            this._context = context;
        }


        [Authorize]
        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] CompanyCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }
            var id = User.Claims.First(c => c.Type == ClaimTypes.IsPersistent).Value;
            var owner = await _context.AppUsers.FirstAsync(u => u.Id == Guid.Parse(id));
            await _context.Companies.AddAsync(new Company
            {
                Name = model.Name,
                Owner = owner
            });
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<CompaniesOwnerVm>>> GetAll()
        {
            var companies = new List<Company>();
            var id = User.Claims.First(c => c.Type == ClaimTypes.IsPersistent).Value;
            var safes = await _context.Safes
                .Include(s => s.SafeUsers)
                .Where(s => s.SafeUsers.Any(u => u.Id == Guid.Parse(id)))
                .Where(s => s.SafeUsers.Any(su => su.Right == RightEnum.Owner))
                .ToListAsync();

            foreach (var safe in safes)
            {
                var r = await _context.Companies.FirstAsync(c => c.Id == safe.CompanyId);
                if (!companies.Contains(r))
                {
                    companies.Add(r);
                }
            }

            var result = new List<CompaniesOwnerVm>();
            foreach (var com in companies)
            {
                result.Add(com.MapToVm());
            }

            return Ok(companies);
        }
    }
}
