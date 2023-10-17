using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Passwork.ClientModule.Utils
{
    internal static class Jwt
    {
        public static AuthenticationState GetStateFromJwt(string token)
            => new(new ClaimsPrincipal(GetIdentityFromJwtToken(token)));

        private static ClaimsIdentity GetIdentityFromJwtToken(string token)
            => new(ParseClaimsFromJwt(token), "jwt");

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
            => new JwtSecurityTokenHandler().ReadJwtToken(jwt).Claims;
    }
}