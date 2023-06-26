using Microsoft.AspNetCore.SignalR.Client;

public class CompanyService
{
    private readonly HubConnection _hubConnection;

    public event Action OnCompanyUpdated;

    public CompanyService()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7292/companyhub")
            .Build();

        _hubConnection.On("CompanyUpdated", () =>
        {
            OnCompanyUpdated?.Invoke();
        });

        _hubConnection.StartAsync();
    }
}
