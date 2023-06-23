using System.Security.Claims;
using System.Text.Json;

namespace Passwork.Client.Services;

/// <summary>
/// Кастомный <see cref="AuthenticationStateProvider"/>.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _authenticationService;

    public CustomAuthStateProvider(TokenService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    /// <summary>
    /// Получить состояние аутентификации.
    /// </summary>
    /// <returns><see cref="Task{TResult}">Task&lt;AuthenticationState&gt;</see></returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = new AuthenticationState(new ClaimsPrincipal());
        var token = await _authenticationService.GetTokenAsync();
        //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzdHJpbmcxQG1haWwiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2lzcGVyc2lzdGVudCI6IjE0OTBkNmFiLTVmZGQtNGFlMS1iOWI1LWNkOWFmMDdkZmU3NyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE2ODc2MzE0MDIsImlzcyI6InBhc0NvbSIsImF1ZCI6ImJsYXpvciJ9.tdcBvfT6xM382tgONR83wSHPY-8LCmCcLM6s9fsgmi4";
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "myjwt");
                var user = new ClaimsPrincipal(new ClaimsIdentity(identity));
                state = new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetAuthenticationStateAsync: " + ex.Message);
            }

        }
        NotifyAuthenticationStateChanged(Task.FromResult(state));

        return state;
    }

    /// <summary>
    /// Парсер клэймов из Json Web Token.
    /// </summary>
    /// <param name="jwt">Json Web Token.</param>
    /// <returns><see cref="List{T}">List&lt;Claim&gt;</see></returns>
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    /// <summary>
    /// Парсинг токена в байтах.
    /// </summary>
    /// <param name="base64"></param>
    /// <returns></returns>
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}