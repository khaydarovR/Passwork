using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Services.SignalR;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Server.Domain;
using Passwork.Server.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.SignalR;
using Passwork.Shared.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Passwork.Server.Application.Services;

public interface IPasswordService
{
    Task<ServiceResponse<bool>> CreatePassword(PasswordCreateDto model, Guid userId);
    Task<ServiceResponse<List<PasswordVm>>> GetPasswords(Guid safeId, Guid userId);
    Task<ServiceResponse<PasswordDetailVm>> GetPasswordDetail(Guid pwId, Guid userId);
    Task<ServiceResponse<bool>> EditPassword(PasswordDetailVm changedPw, Guid userId);
}

public class PasswordService : IPasswordService
{
    private readonly AppDbContext _context;
    private readonly ApiHub _apiHub;
    private readonly ILogger<PasswordService> _logger;
    private readonly TgBotService _tgbot;

    public PasswordService(AppDbContext context,
                           ApiHub apiHub,
                           ILogger<PasswordService> logger,
                           TgBotService tgbot)
    {
        _context = context;
        _apiHub = apiHub;
        _logger = logger;
        _tgbot = tgbot;
    }


    public async Task<ServiceResponse<bool>> CreatePassword(PasswordCreateDto model, Guid userId)
    {
        if (model.Tags?.Count > 5)
        {
            return new ServiceResponse<bool>("Максимально допустимое количество тегов - 5");
        }

        var currentUserRight = (await _context.SafeUsers
            .SingleAsync(su => su.AppUserId == userId && su.SafeId == model.SafeId)).Right;

        if (currentUserRight < RightEnum.Write)
        {
            return new ServiceResponse<bool>($"[{currentUserRight.MapToVm()}] Не достаточно прав для записи новых паролей");
        }

        var safeOwnerId = (await _context.SafeUsers
            .SingleAsync(su => su.Right == RightEnum.Owner && su.SafeId == model.SafeId)).AppUserId;

        var masterUser = await _context.AppUsers
            .Where(u => u.Id == safeOwnerId)
            .SingleAsync();

        var newPassword = new Password
        {
            Title = model.Title,
            Login = Encryptor.Encrypt(model.Login, masterUser.MasterPassword),
            Pw = Encryptor.Encrypt(model.Pw, masterUser.MasterPassword),
            Note = model.Note,
            SafeId = model.SafeId,
            UseInUtl = model.UseInUrl,
            IsDeleted = false,
        };
        await _context.Passwords.AddAsync(newPassword);
        _context.SaveChanges();

        await AddTags(model, newPassword);

        await NotifyCreatedPw(model, userId, currentUserRight, masterUser, newPassword);

        return new ServiceResponse<bool>(true);
    }

    private async Task AddTags(PasswordCreateDto model, Password newPassword)
    {
        var tags = model.Tags?.Select(tag => new Tag { Title = tag }).ToList();
        await _context.Tags.AddRangeAsync(tags);
        _context.SaveChanges();

        var pasTags = new List<PasswordTags>();
        foreach (var tag in tags)
        {
            pasTags.Add(new PasswordTags() { PasswordId = newPassword.Id, TagId = tag.Id });
        }
        await _context.PasswordTags.AddRangeAsync(pasTags);
        _context.SaveChanges();
    }

    private async Task NotifyCreatedPw(PasswordCreateDto model, Guid userId, RightEnum currentUserRight, AppUser masterUser, Password newPassword)
    {
        var currentAppUser = await _context.AppUsers.SingleAsync(su => su.Id == userId);
        _logger.LogInformation($"Created new pw in safe({model.SafeId}, user({currentAppUser.Email})({currentAppUser})", newPassword);

        await _apiHub.SendSignal(EventsEnum.PasswordUpdated, userId.ToString());
        await AddActivityLog(ActivityNames.Created, newPassword.Id, userId);

        await _tgbot.Notify($"Создан новый пароль в сейфе {model.SafeId}," +
            $"\n Текущее право пользовтеля {currentAppUser.Email} - {currentUserRight.MapToVm()} \n Название записи: {model.Title}", masterUser.Email);
    }

    public async Task<ServiceResponse<List<PasswordVm>>> GetPasswords(Guid safeId, Guid userId)
    {
        var response = new ServiceResponse<List<PasswordVm>>();
        if (safeId == Guid.Empty)
        {
            response.ErrorMessage.Message = "Не указан guid сейфа";
            return response;
        }

        var currentUser = await _context.AppUsers.FirstAsync(u => u.Id == userId);

        var currentUserRight = (await _context.SafeUsers.SingleAsync(su => su.AppUserId == currentUser.Id && su.SafeId == safeId)).Right;

        if (currentUserRight < RightEnum.Visible)
        {
            response.ErrorMessage.Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для просмотра записей в сейфе";
            return response;
        }

        response.ResponseModel = new List<PasswordVm>();

        var passwords = await _context.Passwords
            .Include(p => p.PasswordTags)
            .ThenInclude(p => p.Tag)
            .Where(p => p.SafeId == safeId)
            .Where(p => p.IsDeleted == false)
            .ToListAsync();

        foreach (var p in passwords)
        {
            response.ResponseModel.Add(p.MapToVm());
        }

        return response;
    }


    public async Task<ServiceResponse<PasswordDetailVm>> GetPasswordDetail(Guid pwId, Guid userId)
    {
        var response = new ServiceResponse<PasswordDetailVm>();
        var result = new PasswordDetailVm();

        var safeId = (await _context.Passwords
            .SingleAsync(p => p.Id == pwId)).SafeId;

        var safeOwnerId = (await _context.SafeUsers
            .SingleAsync(su => su.Right == RightEnum.Owner && su.SafeId == safeId)).AppUserId;

        var currentUserRight = await _context.SafeUsers
            .Where(su => su.AppUserId == userId)
            .Where(su => su.SafeId == safeId)
            .Select(su => su.Right)
            .SingleAsync();

        if (currentUserRight < RightEnum.Read)
        {
            response.ErrorMessage.Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для чтения";
            return response;
        }

        var passwordDetail = await _context.Passwords
            .Include(p => p.PasswordTags)
            .ThenInclude(p => p.Tag)
            .SingleAsync(p => p.Id == pwId);

        var safeOwner = await _context.AppUsers
            .SingleAsync(u => u.Id == safeOwnerId);

        response.ResponseModel = passwordDetail.MapToDetailVm(safeOwner.MasterPassword);

        await AddActivityLog(ActivityNames.ReceivedDetailData, pwId, userId);

        _logger.LogInformation($"User({userId})({currentUserRight}) get pw detail");

        return response;
    }


    public async Task<ServiceResponse<bool>> EditPassword(PasswordDetailVm changedPw, Guid userId)
    {
        var response = new ServiceResponse<bool>();
        var currentUserRight = await _context.SafeUsers
               .Where(su => su.AppUserId == userId)
               .Where(su => su.SafeId == changedPw.SafeId)
               .Select(su => su.Right)
               .SingleAsync();

        if (currentUserRight < RightEnum.Write)
        {
            response.ErrorMessage.Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для изминения";
            return response;
        }

        if (currentUserRight < RightEnum.Delete && changedPw.IsDeleted == true)
        {
            response.ErrorMessage.Message = $"[{currentUserRight.MapToVm()}] Не достаточно прав для удаления";
            return response;
        }

        var pwDb = await _context.Passwords.SingleAsync(p => p.Id == changedPw.Id);

        var safeOwnerId = (await _context.SafeUsers
            .SingleAsync(su => su.Right == RightEnum.Owner && su.SafeId == changedPw.SafeId)).AppUserId;

        var masterUser = await _context.AppUsers
            .SingleAsync(u => u.Id == safeOwnerId);

        var oldValuePw = Encryptor.Decrypt(pwDb.Pw, masterUser.MasterPassword);
        var oldValueLogin = Encryptor.Decrypt(pwDb.Login, masterUser.MasterPassword);

        if (changedPw.IsDeleted)
        {
            pwDb.IsDeleted = true;
            _context.Passwords.Update(pwDb);
            await _apiHub.SendSignal(EventsEnum.PasswordUpdated, userId.ToString());
            await AddActivityLog(ActivityNames.SoftDeleted, pwDb.Id, userId);
            _logger.LogInformation($"User({userId})({currentUserRight}) delete pw({pwDb.Id}) [old value {oldValueLogin}/{oldValuePw}]");
            return response;
        }

        pwDb.Login = Encryptor.Encrypt(changedPw.Login, masterUser.MasterPassword);
        pwDb.Pw = Encryptor.Encrypt(changedPw.Pw, masterUser.MasterPassword);
        pwDb.Note = changedPw?.Note;
        pwDb.UseInUtl = changedPw?.UseInUtl;
        pwDb.Title = changedPw?.Title;
        _context.Passwords.Update(pwDb);

        await _apiHub.SendSignal(EventsEnum.PasswordUpdated, userId.ToString());
        await AddActivityLog(ActivityNames.Updated, pwDb.Id, userId);

        _logger.LogInformation($"User({userId})({currentUserRight}) edit pw({pwDb.Id}) [old value {oldValueLogin}/{oldValuePw}]");
        return response;
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
        _logger.LogDebug($"Activity log added: ({title})");
    }
}
