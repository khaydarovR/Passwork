﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace Passwork.ClientModule.Services;

/// <summary>
/// Кастомный <see cref="AuthenticationStateProvider"/>.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _authenticationService;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly NavigationManager _navigationManager;

    public CustomAuthStateProvider(TokenService authenticationService, ILogger<CustomAuthStateProvider> logger, NavigationManager navigationManager)
    {
        _authenticationService = authenticationService;
        _logger = logger;
        _navigationManager = navigationManager;
    }
    /// <summary>
    /// Получить состояние аутентификации.
    /// </summary>
    /// <returns><see cref="Task{TResult}">Task&lt;AuthenticationState&gt;</see></returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = new AuthenticationState(new ClaimsPrincipal());
        var token = await _authenticationService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "token");
                var user = new ClaimsPrincipal(new ClaimsIdentity(identity));
                state = new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetAuthenticationStateAsync: " + ex.Message);
            }
            _navigationManager.NavigateTo("/");
        }
        else
        {
            _navigationManager.NavigateTo("/login");
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