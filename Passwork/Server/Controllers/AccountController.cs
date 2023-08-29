using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Passwork.Server.Application;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(AppDbContext dbContext, ILogger<AccountController> logger,
            IAccountService accountService, SignInManager<AppUser> signInManager)
        {
            _dbContext = dbContext;
            this._logger = logger;
            this._accountService = accountService;
            _signInManager = signInManager;
        }


        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Выполните все требования к данным");
            }

            var response = await _accountService.RegisterNewUser(model);
            if (response.IsSuccessful)
            {
                return Ok(response.ResponseModel);
            }

            return BadRequest(response.ErrorMessage);
        }


        //[HttpPost("Login")]
        //public async Task<ActionResult> Login([FromBody] UserLoginDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    var response = await _accountService.LoginUser(model);
        //    if (response.IsSuccessful)
        //    {
        //        return Ok(response.ResponseModel);
        //    }

        //    return BadRequest(response.ErrorMessage);
        //}

        [AllowAnonymous]
        [HttpGet("Current")]
        public async Task<ActionResult<CurrentUser>> Current()
        {
            var serviceResponse = await _accountService.GetUserDetail(User);
            if (serviceResponse.IsSuccessful)
            {
                var userCurrent = new CurrentUser
                {
                    Email = serviceResponse.ResponseModel.Email,
                    Id = serviceResponse.ResponseModel.Id.ToString()
                };
                return Ok(userCurrent);
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("CLogin")]
        public async Task<ActionResult> CLogin(UserLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync
                        (model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded == false)
            {
                return BadRequest("Ошибка при установке куки");
            }
            var dbUser = _dbContext.AppUsers.FirstOrDefault(u => u.Email == model.Email);

            var idClaim = new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString());
            var claimsIdentity = new ClaimsIdentity(new[] { idClaim }, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            return Ok("Cookis set successful");
        }

        [HttpPost("Logout")]
        public async Task<ActionResult> Logut(bool isLogout)
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return Ok();
        }
    }
}
