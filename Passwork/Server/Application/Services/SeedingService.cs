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
    private readonly AppDbContext dbContext;
    private readonly ILogger logger;
    private readonly UserManager<AppUser> userManager;

    public SeedingService(AppDbContext dbContext, ILogger<SeedingService> logger, UserManager<AppUser> userManager)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.userManager = userManager;
    }
    public void DbInit(bool isSeed)
    {
        if (!isSeed)
        {
            logger.LogWarning("Seeding disabled");
            return;
        }

        InitAppUserWithClaims().Wait();

    }

    private async Task InitClaims()
    {
        var users = await dbContext.AppUsers.ToArrayAsync();

        foreach (var user in users)
        {
            var res = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.IsPersistent, user.Id.ToString()));
            var res2 = await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, RoleEmum.User.ToString()));
            if(!res.Succeeded || !res2.Succeeded)
            {
                logger.LogError("Claims can not added");
                return;
            }
        }
    }

    private async Task InitAppUserWithClaims()
    {
        if (dbContext.AppUsers.Any())
        {
            logger.LogWarning("Default users is exists");
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
            var res = await userManager.CreateAsync(user, user.MasterPassword);
            if (res.Succeeded)
            {
                logger.LogDebug(user.Email + " add DB");
            }
            logger.LogError(user.Email + " can not add DB");
        }

        InitClaims().Wait();

    }
}
