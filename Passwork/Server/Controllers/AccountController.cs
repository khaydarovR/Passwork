using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;

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


        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = await _accountService.LoginUser(model);
            if (response.IsSuccessful)
            {
                return Ok(response.ResponseModel);
            }

            return BadRequest(response.ErrorMessage);
        }
    }
}
