using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Passwork.Client.Utils;
using Passwork.Shared.Dto;
using System.Net;
using System.Net.Http.Json;

namespace Passwork.Client.Services;

/// <summary>
/// Менеджер для контроллера аутентификации.
/// </summary>
public class AuthenticationManager
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly TokenService _authenticationService;

    public string ErrorMessage { get; set; } = string.Empty;

    public AuthenticationManager(
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        TokenService authenticationService
        )
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Запрос для аутентификации.
    /// </summary>
    /// <param name="request"><see cref="LoginRequestDto"/></param>
    public async Task<bool> Login(UserLoginDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Routes.Account.Login, request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return false;
        }
        var token = await response.Content.ReadAsStringAsync();
        await _authenticationService.SetTokenAsync(token);
        await _authenticationStateProvider.GetAuthenticationStateAsync();

        return true;
    }

    /// <summary>
    /// Запрос для регистрации.
    /// </summary>
    /// <param name="request"><see cref="UserRegisterDto"/></param>
    public async Task<bool> Register(UserRegisterDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(Routes.Account.Register, request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            ErrorMessage = (await response.Content.ReadFromJsonAsync<ServerResponseError>())?.Message ?? "Ошибка";
            return false;
        }
        var token = (await response.Content.ReadAsStringAsync())?? string.Empty;
        await _authenticationService.SetTokenAsync(token);
        await _authenticationStateProvider.GetAuthenticationStateAsync();
        return true;
    }

    /// <summary>
    /// Выйти из пользователя.
    /// </summary>
    public async Task Logout()
    {
        await _authenticationService.DeleteTokenAsync();
        await _authenticationStateProvider.GetAuthenticationStateAsync();
    }
}