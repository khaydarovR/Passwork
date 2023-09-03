using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Passwork.Client.Services;
using Passwork.Client.Utils;
using Passwork.Shared.SignalR;

public class HubClient
{
    private HubConnection _hubConnection;
    private readonly NavigationManager _navigationManager;
    private readonly TokenService _tokenService;
    private bool _isInitialized;
    public event Action OnCloseConnection;

    public event Action OnCompanyUpdated;
    public event Action OnPassworUpdated;
    public event Action OnSafeUsersUpdated;

    public HubClient(
        NavigationManager navigationManager,
        TokenService tokenService)
    {
        _navigationManager = navigationManager;
        _tokenService = tokenService;
    }

    public async Task StartAsync()
    {
        if (_isInitialized)
        {
            return;
        }
#error не работает hub
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7292/companyhub", options =>
            {
                var jwt = _tokenService.GetTokenAsync().Result;
                options.Headers.Add("Authorization", "Bearer " + jwt.Trim('"'));
            })
            .Build();

        _hubConnection.Closed += HubConnectionClosed!;

        _hubConnection.On(EventsEnum.CompanyUpdated.ToString(), () =>
        {
            OnCompanyUpdated?.Invoke();
        });

        _hubConnection.On(EventsEnum.PasswordUpdated.ToString(), () =>
        {
            OnPassworUpdated?.Invoke();
        });

        _hubConnection.On(EventsEnum.SafeUserUpdated.ToString(), () =>
        {
            OnSafeUsersUpdated?.Invoke();
        });

        try
        {
            await _hubConnection.StartAsync();
            _isInitialized = true;
        }
        catch
        {
            if(_tokenService.GetTokenAsync().Result == null)
            {
                _navigationManager.NavigateTo("/register", true);
            }
            _isInitialized = false;
        }
    }

    private async Task HubConnectionClosed(Exception ex)
    {
        OnCloseConnection?.Invoke();

        await Task.CompletedTask;
    }
}
