﻿using Passwork.Client.Utils;
using Passwork.Shared.Dto;
using Passwork.Shared.ViewModels;
using System.Net;
using System.Net.Http.Json;

namespace Passwork.Client.Services;

/// <summary>
/// Менеджер для контроллера аутентификации.
/// </summary>
public class AuthenticationManager
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authenticationStateProvider;
    private readonly TokenService _authenticationService;
    private readonly HubClient _hubClient;
    private readonly ApiService _apiService;

    public string ErrorMessage { get; set; } = string.Empty;

    public AuthenticationManager(
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        TokenService authenticationService,
        HubClient hubClient,
        ApiService apiService)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
        _authenticationService = authenticationService;
        this._hubClient = hubClient;
        _apiService = apiService;
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
            ErrorMessage = (await response.Content.ReadFromJsonAsync<ErrorMessage>())?.Message ?? "Ошибка на сервере";
            return false;
        }
        var token = await response.Content.ReadAsStringAsync();
        _authenticationService.SetToken(token);
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
            ErrorMessage = (await response.Content.ReadFromJsonAsync<ErrorMessage>())?.Message ?? "Ошибка на сервере";
            return false;
        }
        var token = (await response.Content.ReadAsStringAsync()) ?? string.Empty;
        _authenticationService.SetToken(token);
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