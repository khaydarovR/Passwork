using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Passwork.Server.Application.Interfaces;
using Passwork.Server.DAL;
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

        public AccountController(AppDbContext dbContext, ILogger<AccountController> logger,
            IAccountService accountService)
        {
            _dbContext = dbContext;
            this._logger = logger;
            this._accountService = accountService;
        }


        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var response = await _accountService.RegisterNewUser(model);
            if (response.IsSuccessful)
            {
                return Ok(response.ResponseModel);
            }

            foreach (var e in response.Errors)
            {
                ModelState.AddModelError("", e);
            }
            return BadRequest();
        }
    }
}
