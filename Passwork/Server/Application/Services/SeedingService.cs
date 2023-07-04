using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using System.Security.Claims;

namespace Passwork.Server.Application.Services;

public class SeedingService : ISeedingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserManager<AppUser> _userManager;

    public SeedingService(AppDbContext dbContext, ILogger<SeedingService> logger, UserManager<AppUser> userManager)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._userManager = userManager;
    }
    public void DbInit(bool isSeed)
    {
        if (!isSeed)
        {
            _logger.LogWarning("Seeding disabled");
            return;
        }

        InitAppUserWithClaims().Wait();
        InitCompany().Wait();

    }

    private async Task InitCompany()
    {
        if (_dbContext.Companies.Any())
        {
            _logger.LogWarning("Default company is exists");
            return;
        }

        var template = new List<Company>
        {
            new()
            {
                Name = "ООО ЦИТ",
            },
            new()
            {
                Name = "Google",
            },
            new()
            {
                Name = "Камаз"
            }
        };

        await _dbContext.Companies.AddRangeAsync(template);
        _dbContext.SaveChangesAsync().Wait();
        _logger.LogDebug("Сompanies added to the database");
    }


    private async Task InitClaims()
    {
        var users = await _dbContext.AppUsers.ToArrayAsync();

        foreach (var user in users)
        {
            var res = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.IsPersistent, user.Id.ToString()));
            var res2 = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, RoleEmum.User.ToString()));
            if (!res.Succeeded || !res2.Succeeded)
            {
                _logger.LogError("Claims can not be added");
                return;
            }
        }
    }


    private async Task InitAppUserWithClaims()
    {
        if (_dbContext.AppUsers.Any())
        {
            _logger.LogWarning("Default users is exists");
            return;
        }

        var usersTemplate = new List<AppUser>
        {
            new()
            {
                MasterPassword = "maspas1",
                Email = "test1@mail.ru",
                UserName = "test1",
            },
            new()
            {
                MasterPassword = "maspas2",
                Email = "test2@mail.ru",
                UserName = "test2"
            },
            new()
            {
                MasterPassword = "maspas3",
                Email = "test3@mail.ru",
                UserName = "test3"
            },
        };

        foreach (var user in usersTemplate)
        {
            var res = await _userManager.CreateAsync(user, user.MasterPassword);
            if (res.Succeeded)
            {
                _logger.LogDebug(user.Email + " user added to the database");
            }
            _logger.LogError(user.Email + " can not to be add DB");
        }

        InitClaims().Wait();
    }

    public void DropCreateDb()
    {
        Console.WriteLine("Вы действительно хотите пересоздать все таблицы?: y - yes, n - no");
        if(Console.ReadKey().KeyChar == 'y')
        {
            _dbContext.Database.EnsureDeleted();

            _dbContext.Database.EnsureCreated();
            _logger.LogWarning("БД был удален и пересоздан");
        }
        _logger.LogWarning("Отмена операции");
    }
}
