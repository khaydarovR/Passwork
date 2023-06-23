using Passwork.Server.Application.Services;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.Security.Claims;

namespace Passwork.Server.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<AppUser>> ExitFromAccount();
        Task<Response<AppUser>> GetUserDetail(ClaimsPrincipal claimsPrincipal);
        Task<Response<string>> LoginUser(UserLoginDto model);
        Task<Response<string>> RegisterNewUser(UserRegisterDto model);
    }
}