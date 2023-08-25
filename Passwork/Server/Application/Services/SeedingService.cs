using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Passwork.Server.Application.Services;

public class SeedingService : ISeedingService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IAccountService _accountService;

    public SeedingService(AppDbContext dbContext, ILogger<SeedingService> logger, UserManager<AppUser> userManager, IAccountService accountService)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._userManager = userManager;
        this._accountService = accountService;
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
        InitUsers(50).Wait();
    }

    private async Task InitUsers(int amount)
    {
        if (_dbContext.AppUsers.Any(u => u.Email == "t0@ma"))
        {
            _logger.LogWarning("Default users is exists");
            return;
        }

        var newUsers = new List<UserRegisterDto>();
        for (int i = 0; i < amount; i++)
        {
            var email = $"t{i}@ma";
            newUsers.Add(new UserRegisterDto
            {
                Email = email,
                MasterPassword = $"masterPas{i}",
                Password = $"tpas{i}"
            });

            _accountService.RegisterNewUser(newUsers[i]);
        }
        _logger.LogDebug("Users added to the database");
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
