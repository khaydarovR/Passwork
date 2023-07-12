using Passwork.Server.Domain.Entity;
using Passwork.Shared.Dto;
using System.Security.Claims;

namespace Passwork.Server.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResponse<AppUser>> GetUserDetail(ClaimsPrincipal claimsPrincipal);
        Task<ServiceResponse<string>> LoginUser(UserLoginDto model);
        Task<ServiceResponse<string>> RegisterNewUser(UserRegisterDto model);
    }
}