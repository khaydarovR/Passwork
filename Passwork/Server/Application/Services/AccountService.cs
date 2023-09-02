using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Passwork.Server.Application.Configure;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Passwork.Server.Application.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<AccountService> _logger;
    private readonly IDeferredInviter _inviter;

    public AccountService(
        UserManager<AppUser> userManager,
        AppDbContext context,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<AccountService> logger,
        IDeferredInviter inviter)
    {
        _userManager = userManager;
        _context = context;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
        _inviter = inviter;
    }


    public async Task<ServiceResponse<string>> RegisterNewUser(UserRegisterDto model)
    {
        var newUser = await MapToAppUser(model);

        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(newUser, true);
            var newUserDb = await _userManager.Users.SingleAsync(u => u.Email == model.Email);
            var claims = GetClaimsFor(newUserDb);
            await _userManager.AddClaimsAsync(newUserDb, claims);
            var jwt = CreateJwt(claims);
            _logger.LogInformation("Created new app user", newUserDb);

            TryAddToSafeNewUser(newUserDb);
            return new ServiceResponse<string>(true) { ResponseModel = new JwtSecurityTokenHandler().WriteToken(jwt) };
        }

        var badResponse = new ServiceResponse<string>(false);
        badResponse.ErrorMessage = new ErrorMessage(result.Errors.First().Description);
        return badResponse;
    }

    private async void TryAddToSafeNewUser(AppUser newUser)
    {
        if (await _inviter.ValueIsExists(newUser.Email) == false)
        {
            return;
        }

        var safeId = await _inviter.GetSafeAndRemoveValue(newUser.Email);
        var addToSafe = new SafeUsers()
        {
            AppUser = newUser,
            SafeId = Guid.Parse(safeId),
            Right = RightEnum.Visible
        };
        _context.SafeUsers.Add(addToSafe);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceResponse<string>> LoginUser(UserLoginDto model)
    {
        var result = await _signInManager.PasswordSignInAsync
            (model.Email, model.Password, true, false);

        if (result.Succeeded)
        {
            var userDb = await _userManager.Users.SingleAsync(u => u.Email == model.Email);
            var claims = await _userManager.GetClaimsAsync(userDb);
            var jwt = CreateJwt(claims.ToList());
            return new ServiceResponse<string>(true) { ResponseModel = new JwtSecurityTokenHandler().WriteToken(jwt) };
        }

        return new ServiceResponse<string>("Ошибка авторизации", false);
    }

    public async Task<ServiceResponse<AppUser>> GetUserDetail(ClaimsPrincipal claimsPrincipal)
    {
        var result = await _userManager.GetUserAsync(claimsPrincipal);

        return new ServiceResponse<AppUser>("Ошибка в куках, зарегистрируйтесть заного", false);
    }


    private async Task<AppUser> MapToAppUser(UserRegisterDto model)
    {
        var newUser = new AppUser()
        {
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = false,
            MasterPassword = model.MasterPassword,
            CreatedDate = DateTime.UtcNow,
            SecurityStamp = DateTime.Now.ToLongTimeString()
        };
        return newUser;
    }

    private static JwtSecurityToken CreateJwt(List<Claim> claims)
    {
        var jwt = new JwtSecurityToken(
            issuer: JwtOptions.ISSUER,
            audience: JwtOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
            signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(),
                                                    SecurityAlgorithms.HmacSha256)
        );
        return jwt;
    }

    private static List<Claim> GetClaimsFor(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.IsPersistent, user.Id.ToString()),
            new Claim(ClaimTypes.Role, RoleEmum.User.ToString())
        };
        return claims;
    }
}
