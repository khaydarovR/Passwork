using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.SignalR;
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
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(AppDbContext context, ApiHub apiHub, ILogger<PasswordController> logger)
        {
            _context = context;
            _apiHub = apiHub;
            _logger = logger;
        }


        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] PasswordCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorMessage { Message = "Не валидные данные" });
            }
            if (model.Tags?.Count > 5)
            {
                return BadRequest( new ErrorMessage { Message = "Максимально допустимое количество тегов - 5" });
            }
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;
            var currentUser = await _context.AppUsers.FirstAsync(u => u.Id == Guid.Parse(id));

            var currentUserRight = (await _context.SafeUsers
                .SingleAsync(su => su.AppUserId == currentUser.Id && su.SafeId == model.SafeId)).Right;

            if (currentUserRight < RightEnum.Write)
            {
                return BadRequest(new ErrorMessage { Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для записи новых паролей" });
            }

            var safeOwnerId = (await _context.SafeUsers
                .SingleAsync(su => su.Right == RightEnum.Owner && su.SafeId == model.SafeId)).AppUserId;

            var masterPw = await _context.AppUsers
                .Where(u => u.Id == safeOwnerId)
                .Select(u => u.MasterPassword)
                .SingleAsync();

            var newPassword = new Password
            {
                Title = model.Title,
                Login = Encryptor.Encrypt(masterPw, model.Login),
                Pw = Encryptor.Encrypt(masterPw, model.Pw),
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

            await _apiHub.SendSignal(EventsEnum.PasswordUpdated, id);
            await AddActivityLog(ActivityNames.Created, newPassword.Id, Guid.Parse(id));

            return Ok();
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<List<PasswordVm>>> Get([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }

            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;
            var currentUser = await _context.AppUsers.FirstAsync(u => u.Id == Guid.Parse(id));

            var currentUserRight = (await _context.SafeUsers.SingleAsync(su => su.AppUserId == currentUser.Id && su.SafeId == safeId)).Right;

            if(currentUserRight < RightEnum.Visible)
            {
                return BadRequest(new ErrorMessage { Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для просмотра записей в сейфе" });
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


        [HttpGet("Detail")]
        public async Task<ActionResult<PasswordDetailVm>> Detail([FromQuery] Guid pwId)
        {
            if (pwId == Guid.Empty)
            {
                return BadRequest(new ErrorMessage { Message = "Не выбран пароль"});
            }
            var result = new PasswordDetailVm();

            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var safeId = (await _context.Passwords
                .SingleAsync(p => p.Id == pwId)).SafeId;

            var safeOwnerId = (await _context.SafeUsers
                .SingleAsync(su => su.Right == RightEnum.Owner && su.SafeId == safeId)).AppUserId;

            var currentUserRight = await _context.SafeUsers
                .Where(su => su.AppUserId == Guid.Parse(userId))
                .Where(su => su.SafeId == safeId)
                .Select(su => su.Right)
                .SingleAsync();

            if (currentUserRight < RightEnum.Read)
            {
                return BadRequest(new ErrorMessage { Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для чтения"});
            }

            var passwordDetail = await _context.Passwords
                .Include(p => p.PasswordTags)
                .ThenInclude(p => p.Tag)
                .SingleAsync(p => p.Id == pwId);

            var masterPw = await _context.AppUsers
                .Where(u => u.Id == safeOwnerId)
                .Select(u => u.MasterPassword)
                .SingleAsync();
            result = passwordDetail.MapToDetailVm(masterPw);

            await AddActivityLog(ActivityNames.ReceivedDetailData, pwId, Guid.Parse(userId));

            return Ok(result);
        }

        private async Task AddActivityLog(string title, Guid pwId, Guid userId)
        {
            var newLog = new ActivityLog()
            {
                AppUsreId = userId,
                PasswordId = pwId,
                Title = title,
                At = DateTime.UtcNow,
            };
            _context.ActivityLogs.Add(newLog);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Activity log added");
        }
    }
}
