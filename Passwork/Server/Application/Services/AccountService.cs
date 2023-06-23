using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Passwork.Server.Application.Configure;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Passwork.Server.Application.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;


    public AccountService(
        UserManager<AppUser> userManager,
        AppDbContext context,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }


    public async Task<Response<string>> RegisterNewUser(UserRegisterDto model)
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
            return new Response<string>(true) { ResponseModel = new JwtSecurityTokenHandler().WriteToken(jwt) };
        }

        var badResponse = new Response<string>(false);
        foreach (var error in result.Errors)
        {
            badResponse.Errors.Add(error.Description);
        }
        return badResponse;
    }

    public async Task<Response<AppUser>> LoginUser(UserLoginDto model)
    {
        var result = await _signInManager.PasswordSignInAsync
            (model.Email, model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            return new Response<AppUser>(await _userManager.Users.FirstAsync(u => u.Email == model.Email), true);
        }

        return new Response<AppUser>("Ошибка авторизации", false);
    }

    public async Task<Response<AppUser>> GetUserDetail(ClaimsPrincipal claimsPrincipal)
    {
        var result = await _userManager.GetUserAsync(claimsPrincipal);

        await _signInManager.SignOutAsync();
        return new Response<AppUser>("Ошибка в куках, зарегистрируйтесть заного", false);
    }


    public async Task<Response<AppUser>> ExitFromAccount()
    {
        await _signInManager.SignOutAsync();
        return new Response<AppUser>(true);
    }

    private async Task<AppUser> MapToAppUser(UserRegisterDto model)
    {
        var newUser = new AppUser()
        {
            Email = model.Email,
            UserName = model.Email,
            //UserState = await _context.UserState.FirstAsync(s => s.State == StateEnum.Active),
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
