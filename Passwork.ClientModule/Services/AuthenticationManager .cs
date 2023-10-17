using Microsoft.AspNetCore.Components.Authorization;
using Passwork.ClientModule.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.Net;
using System.Net.Http.Json;

namespace Passwork.ClientModule.Services;

/// <summary>
/// Менеджер для контроллера аутентификации.
/// </summary>
public class AuthenticationManager
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authenticationStateProvider;
    private readonly TokenService _authenticationService;
    private readonly HubClient _hubClient;

    public string ErrorMessage { get; set; } = string.Empty;

    public AuthenticationManager(
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        TokenService authenticationService,
        HubClient hubClient)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
        _authenticationService = authenticationService;
        _hubClient = hubClient;
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
        _authenticationService.SetToken(token);
        await _authenticationStateProvider.GetAuthenticationStateAsync();
        await _hubClient.StartAsync();
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
            ErrorMessage = (await response.Content.ReadFromJsonAsync<ErrorMessage>())?.Message ?? "Ошибка";
            return false;
        }
        var token = await response.Content.ReadAsStringAsync() ?? string.Empty;
        _authenticationService.SetToken(token);
        await _authenticationStateProvider.GetAuthenticationStateAsync();
        await _hubClient.StartAsync();
        return true;
    }

    /// <summary>
    /// Выйти из пользователя.
    /// </summary>
    public async Task Logout()
    {
        await _httpClient.PostAsJsonAsync("/api/Account/Logout", true);
        await _authenticationService.DeleteTokenAsync();
        await _authenticationStateProvider.GetAuthenticationStateAsync();
    }
}