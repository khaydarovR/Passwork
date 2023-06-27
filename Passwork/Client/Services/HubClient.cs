using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Passwork.Client.Services;
using System;

public class HubClient
{
    private HubConnection _hubConnection;
    private readonly CustomAuthStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;
    private bool _isInitialized;


    public event Action OnCompanyUpdated;

    public HubClient(AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager)
    {
        _authenticationStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
        this._navigationManager = navigationManager;
    }

    public async Task StartAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7292/companyhub", options =>
            {
                Func<Task<string?>>? setToken = async () =>
                {
                    var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                    var accessToken = authState.User.FindFirst("access_token")?.Value;
                    return accessToken;
                };
                options.AccessTokenProvider = setToken;
            })
            .Build();

        _hubConnection.Closed += HubConnectionClosed;
        _hubConnection.On("CompanyUpdated", () =>
        {
            OnCompanyUpdated?.Invoke();
        });
        try
        {
            await _hubConnection.StartAsync();
            _isInitialized = true;
        }
        catch
        {
            _navigationManager.NavigateTo("/", true);
            _isInitialized = false;
        }
    }

    private void ConfigureHubConnection(HttpConnectionOptions options)
    {

    }

    private async Task HubConnectionClosed(Exception ex)
    {
        if (ex != null && ex is HubException hubException)
        {
            if (ex.Message == "Unauthorized")
            {
                _navigationManager.NavigateTo("/");
            }
        }

        await Task.CompletedTask;
    }
}
