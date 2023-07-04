using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Passwork.Client.Services;
using Passwork.Shared.SignalR;

public class HubClient
{
    private HubConnection _hubConnection;
    private readonly CustomAuthStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;
    private bool _isInitialized;
    public event Action OnCloseConnection;

    public event Action OnCompanyUpdated;
    public event Action OnPassworUpdated;
    public event Action OnSafeUsersUpdated;

    public HubClient(
        AuthenticationStateProvider authenticationStateProvider, 
        NavigationManager navigationManager)
    {
        _authenticationStateProvider = (CustomAuthStateProvider)authenticationStateProvider;
        _navigationManager = navigationManager;
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
            if((await _authenticationStateProvider.GetAuthenticationStateAsync()).User.Identity.IsAuthenticated == false)
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
