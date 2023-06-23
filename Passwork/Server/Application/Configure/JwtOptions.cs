using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Passwork.Server.Application.Configure
{
    public class JwtOptions
    {
        public const string ISSUER = "pasCom"; // издатель токена
        public const string AUDIENCE = "blazor"; // потребитель токена
        private static string KEY = "LongSecretKey333_danger!!!_warning!!!_need_long_key";
        public static void SetKey(string configKey)
        {
            KEY = configKey;
        }
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
