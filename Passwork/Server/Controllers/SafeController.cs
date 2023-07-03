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
                Right = Domain.RightEnum.Owner,
                SafeId = newSafe.Id
            };
            await _context.SafeUsers.AddAsync(safeUser);

            await _context.SaveChangesAsync();
            await _apiHub.SendCompanyUpdate(id);
            return Ok();
        }


        [HttpGet("Users")]
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


        [HttpPost("AddUser")]
        public async Task<ActionResult> AddUser([FromBody] AddUserToSafeDto addUserToSafeDto)
        {
            var newSafeUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == addUserToSafeDto.UserEmail);
            if (newSafeUser == null)
            {
                return BadRequest(new ErrorMessage { Message = $"Пользователь c почтой {addUserToSafeDto.UserEmail} не зарегистрирован" });
            }


            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var rightCurentUser = await _context.SafeUsers
                .Where(su => su.AppUserId == Guid.Parse(userId))
                .Where(su => su.SafeId == addUserToSafeDto.SafeId)
                .Select(su => su.Right)
                .SingleAsync();

            if (rightCurentUser < RightEnum.Invite)
            {
                return BadRequest(new ErrorMessage { Message = "Не достаточно прав для добавления новых пользователей в сэйф" });
            }
            if (addUserToSafeDto.Right >= RightEnumVm.Владелец)
            {
                return BadRequest(new ErrorMessage { Message = "У сейфа может быть только один владелец" });
            }
            if (rightCurentUser <= addUserToSafeDto.Right.MapToEnum())
            {
                return BadRequest(new ErrorMessage { Message = "Вы не можете предоставить другим пользователям более высокий уровень доступа, чем у вас" });
            }

            var checkExist = await _context.SafeUsers
                .Where(su => su.AppUserId == newSafeUser.Id)
                .Where(su => su.SafeId == addUserToSafeDto.SafeId)
                .AnyAsync();

            if(checkExist)
            {
                return BadRequest(new ErrorMessage { Message = "Данный пользователь уже прикреплен к сейфу" });
            }

            await _context.SafeUsers.AddAsync(new SafeUsers()
            {
                AppUserId = newSafeUser.Id,
                SafeId = addUserToSafeDto.SafeId,
                Right = addUserToSafeDto.Right.MapToEnum()
            });

            //hub send signal
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
