﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.Application.Services;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.SignalR;
using Passwork.Shared.ViewModels;
using System.Linq;
using System.Security.Claims;


namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SafeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiHub _apiHub;
        private readonly TgBotService _tgbot;
        private readonly ILogger<SafeController> _logger;
        private readonly IDeferredInviter _inviter;

        public SafeController(AppDbContext context, 
            ApiHub apiHub, 
            TgBotService tgbot, 
            ILogger<SafeController> logger,
            IDeferredInviter inviter)
        {
            _context = context;
            _apiHub = apiHub;
            _tgbot = tgbot;
            _logger = logger;
            _inviter = inviter;
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

            await _apiHub.SendSignal(EventsEnum.CompanyUpdated, id);
            return Ok();
        }


        [HttpGet("Users")]
        public async Task<ActionResult<List<SafeUserVm>>> Users([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest( new ErrorMessage { Message = "Не указан id сейфа" });
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
                try
                {
                    await _inviter.WaitInvite(addUserToSafeDto.UserEmail, addUserToSafeDto.SafeId.ToString(), TimeSpan.FromDays(1));
                    return BadRequest(new ErrorMessage
                    {
                        Message = $"[Отложенное добавление]{addUserToSafeDto.UserEmail} будет добавлен в сейф после регистрации"
                    });
                }
                catch (Exception e)
                {
                    return BadRequest(new ErrorMessage { Message = "Сервис для отложенного приглашения не доступен" });
                }

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

            _context.SaveChanges();

            await _apiHub.SendSignal(EventsEnum.SafeUserUpdated, userId);
            await _apiHub.SendSignal(EventsEnum.SafeUserUpdated, newSafeUser.Id.ToString());

            _logger.LogInformation($"user: {userId} added to the safe: {addUserToSafeDto.SafeId}");

            return Ok();
        }


        [HttpPost("DeleteUsers")]
        public async Task<ActionResult<List<SafeUserVm>>> DeleteUsers([FromBody] DeleteUsersFromSafeDto usersForDelete)
        {
            if (usersForDelete.SafeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }

            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var rightCurentUser = await _context.SafeUsers
                .Where(su => su.AppUserId == Guid.Parse(userId))
                .Where(su => su.SafeId == usersForDelete.SafeId)
                .Select(su => su.Right)
                .SingleAsync();

            if (rightCurentUser < RightEnum.Delete)
            {
                return BadRequest(new ErrorMessage { Message = "Не достаточно прав для удаления пользователей из сейфа" });
            }
            if (usersForDelete.UserIds.Contains(Guid.Parse(userId)))
            {
                return BadRequest(new ErrorMessage { Message = "Вы не можете самоуничтожиться в сейфе :)" });
            }

            var users = await _context.SafeUsers
                .Where(su => (su.SafeId == usersForDelete.SafeId && usersForDelete.UserIds.Contains(su.AppUserId)))
                .ToListAsync();

            if(users.Contains(users.FirstOrDefault(su => su.Right == RightEnum.Owner)))
            {
                return BadRequest(new ErrorMessage { Message = "Нельзя удалить владельца сейфа" });
            }
            if (users.Count() == 0)
            {
                return BadRequest(new ErrorMessage { Message = "Указанные пользователи не найдены в сейфе" });
            }

            _context.SafeUsers.RemoveRange(users);
            _context.SaveChanges();


            var editedUserIds = users.Select(u => u.AppUserId).ToList();
            await _apiHub.SendSignal(EventsEnum.SafeUserUpdated, userId);
            await _apiHub.SendSignalRange(EventsEnum.SafeUserUpdated, editedUserIds);

            _logger.LogInformation($"Delete users: {users.Count} from safe: {usersForDelete.SafeId}");
            return Ok();
        }


        [HttpPost("ChangeRights")]
        public async Task<ActionResult<List<SafeUserVm>>> ChangeRights([FromBody] ChangeUserRightsDto newRights)
        {
            if (newRights.SafeId == Guid.Empty)
            {
                return BadRequest("Не указан id сейфа");
            }

            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var rightCurentUser = await _context.SafeUsers
                .Where(su => su.AppUserId == Guid.Parse(userId))
                .Where(su => su.SafeId == newRights.SafeId)
                .Select(su => su.Right)
                .SingleAsync();

            if (rightCurentUser < RightEnum.Invite)
            {
                return BadRequest(new ErrorMessage { Message = $"[{rightCurentUser.MapToVm().ToString()}] Не достаточно прав" });
            }
            if (rightCurentUser <= newRights.NewRight.MapToEnum())
            {
                return BadRequest(new ErrorMessage { Message = "Запрещено выдавать право превышающие ваше текущее" });
            }

            var usersForRightChangeDb = await _context.SafeUsers
                .Where(su => (su.SafeId == newRights.SafeId && newRights.UserIds.Contains(su.AppUserId)))
                .ToListAsync();

            var ownerIsExists = usersForRightChangeDb.Any(u => u.Right == RightEnum.Owner);
            if (ownerIsExists)
            {
                return BadRequest(new ErrorMessage { Message = "Запрещено менять права владельца сейфа" });
            }

            foreach (var u in usersForRightChangeDb)
            {
                u.Right = newRights.NewRight.MapToEnum();
            }

            _context.SafeUsers.UpdateRange(usersForRightChangeDb);
            await _context.SaveChangesAsync();

            var editedUserIds = usersForRightChangeDb.Select(u => u.AppUserId).ToList();
            var safeOwnerId = (await _context.SafeUsers.SingleAsync(u => u.Right == RightEnum.Owner && u.SafeId == newRights.SafeId)).AppUserId;
            await _apiHub.SendSignal(EventsEnum.SafeUserUpdated, safeOwnerId.ToString());
            await _apiHub.SendSignal(EventsEnum.SafeUserUpdated, userId);
            await _apiHub.SendSignalRange(EventsEnum.SafeUserUpdated, editedUserIds);

            var currentUser = await _context.AppUsers.SingleAsync(u => u.Id == Guid.Parse(userId));
            await _tgbot.Notify($"Пользователь: {userId} изминил права в сейфе: {newRights.SafeId}", currentUser.Email);
            _logger.LogInformation($"user: {userId} changed rights in safe: {newRights.SafeId}");

            return Ok();
        }


        [HttpGet("Constring")]
        public async Task<ActionResult<string>> ConnectionString([FromQuery] Guid safeId)
        {
            if (safeId == Guid.Empty)
            {
                return BadRequest("Не указан сейф");
            }
            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var curentSafeUser = await _context.SafeUsers
                .Where(su => su.AppUserId == Guid.Parse(userId))
                .Where(su => su.SafeId == safeId)
                .SingleAsync();

            if(curentSafeUser.Right != RightEnum.Owner)
            {
                BadRequest("Не достаточно прав, только для владельца");
            }

            var user = await _context.AppUsers
                .SingleAsync(u => u.Id == curentSafeUser.AppUserId);

            _tgbot.ConnectionString = KeyGenerator.GetRandomString() + " " + user.Email;
            return Ok("key " + _tgbot.ConnectionString);
        }
    }
}
