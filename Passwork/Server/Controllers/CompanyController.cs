using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
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
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompanyController> _logger;
        private readonly ApiHub _apiHub;

        public CompanyController(AppDbContext context, ILogger<CompanyController> logger, ApiHub apiHub)
        {
            this._context = context;
            this._logger = logger;
            this._apiHub = apiHub;
        }


        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] CompanyCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные");
            }

            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            await _context.Companies.AddAsync(new Company
            {
                Name = model.Name,
                AppUserId = Guid.Parse(id)
            });
            await _context.SaveChangesAsync();
            await _apiHub.SendCompanyUpdate(id);
            _logger.LogDebug("Created compnay and send signal to client " + id);

            return Ok();
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<List<CompaniesOwnerVm>>> GetAll()
        {
            var companies = new List<Company>();
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var safes = await _context.Safes
                .Include(s => s.SafeUsers)
                .Where(s => s.SafeUsers.Any(u => u.AppUserId == Guid.Parse(id)))
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

            return Ok(result);
        }


        [HttpGet("OwnerCom")]
        public async Task<ActionResult<List<CompaniesOwnerVm>>> OwnerCom()
        {
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var companies = await _context.Companies
                .Include(s => s.Safes)
                .Where(c => c.AppUserId == Guid.Parse(id))
                .ToListAsync();


            var result = new List<CompaniesOwnerVm>();
            foreach (var com in companies)
            {
                result.Add(com.MapToVm());
            }

            return Ok(result);
        }


        [HttpGet("Users")]
        public async Task<ActionResult<List<ComUserVm>>> Users([FromQuery] Guid safeId)
        {
            var currentSafeId = safeId;
            //Получение связанной компании с сэйфом - получение всех сейфов компании - получение всех пользователей связанныз с сейфами
            if (currentSafeId == Guid.Empty)
            {
                return BadRequest(new ErrorMessage() { Message = "Не указан id сейфа" });
            }

            var comId = await _context.Safes
                .Where(s => s.Id == currentSafeId)
                .Select(s => s.CompanyId)
                .SingleAsync();

            var safeIds = await _context.Companies
                .Where(c => c.Id == comId)
                .SelectMany(c => c.Safes.Select(s => s.Id))
                .ToListAsync();


            var users = await _context.AppUsers
                .Include(u => u.SafeUsers)
                .Where(u => u.SafeUsers.Any(su => safeIds.Contains(su.SafeId)))
                .ToListAsync();

            var result = new List<ComUserVm>();
            foreach (var u in users)
            {
                result.Add(u.MapToComUserVm());
            }

            return Ok(result);
        }
    }
}
