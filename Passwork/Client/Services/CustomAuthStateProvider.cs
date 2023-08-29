using IdentityModel.Client;
using Microsoft.AspNetCore.Components;
using Passwork.Shared.ViewModels;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Passwork.Client.Services;

/// <summary>
/// Кастомный <see cref="AuthenticationStateProvider"/>.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokenService;
    private readonly NavigationManager _navigationManager;


    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUser currentUser = null;
        var authResponse = await _httpClient.GetAsync("api/Account/Current");
        if (authResponse.IsSuccessStatusCode)
        {
            currentUser = await authResponse.Content.ReadFromJsonAsync<CurrentUser>();
        }

        if (currentUser != null && currentUser.Id != null)
        {
            //create a claims
            var claimEmailAddress = new Claim(ClaimTypes.Email, currentUser.Email);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, currentUser.Id);
            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEmailAddress, claimNameIdentifier }, "serverAuth");
            //create claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var newAuthState = new AuthenticationState(claimsPrincipal);
            NotifyAuthenticationStateChanged(Task.FromResult(newAuthState));
            return newAuthState;
        }
        else
        {
            _navigationManager.NavigateTo("./Login");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}