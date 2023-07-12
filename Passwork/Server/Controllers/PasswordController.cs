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
using System.Security.Claims;

namespace Passwork.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ApiHub _apiHub;
        private readonly ILogger<PasswordController> _logger;
        private readonly TgBotService _tgbot;
        private readonly IPasswordService _pwService;

        public PasswordController(AppDbContext context, ApiHub apiHub, ILogger<PasswordController> logger, TgBotService tgbot, IPasswordService pwService)
        {
            _context = context;
            _apiHub = apiHub;
            _logger = logger;
            _tgbot = tgbot;
            _pwService = pwService;
        }


        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] PasswordCreateDto model)
        {
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные: " + ModelState.First().Value);
            }

            var response = await _pwService.CreatePassword(model, Guid.Parse(id));
            if (response.IsSuccessful)
            {
                return Ok();
            }

            return BadRequest(response.ErrorMessage);
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<List<PasswordVm>>> Get([FromQuery] Guid safeId)
        {
            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            if (!ModelState.IsValid)
            {
                return BadRequest("Не валидные данные: " + ModelState.First().Value);
            }

            var response = await _pwService.GetPasswords(safeId, Guid.Parse(id));
            if (response.IsSuccessful)
            {
                return Ok(response.ResponseModel);
            }

            return BadRequest(response.ErrorMessage);
        }


        [HttpGet("Detail")]
        public async Task<ActionResult<PasswordDetailVm>> Detail([FromQuery] Guid pwId)
        {
            if (pwId == Guid.Empty)
            {
                return BadRequest(new ErrorMessage { Message = "Отсутсвует guid пароля"});
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorMessage { Message = "Не валлидные данные: " + ModelState.First().Value });
            }

            var claimsPrincipal = HttpContext.User;
            var id = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var response = await _pwService.GetPasswordDetail(pwId, Guid.Parse(id));
            if (response.IsSuccessful)
            {
                return Ok(response.ResponseModel);
            }

            return BadRequest(response.ErrorMessage);
            
        }


        [HttpPost("Edit")]
        public async Task<ActionResult> Edit([FromBody] PasswordDetailVm changedPw)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorMessage { Message = "Поля заполнены не правльно" });
            }
            var claimsPrincipal = HttpContext.User;
            var userId = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value!;

            var response = await _pwService.EditPassword(changedPw, Guid.Parse(userId));
            if (response.IsSuccessful)
            {
                return Ok();
            }

            return BadRequest(response.ErrorMessage);
        }
    }
}
