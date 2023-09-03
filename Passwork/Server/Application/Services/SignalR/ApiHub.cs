using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MudBlazor;
using Passwork.Shared.SignalR;
using System.Diagnostics;
using System.Security.Claims;

namespace Passwork.Server.Application.Services.SignalR;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiHub> _logger;

    public ApiHub(IHttpContextAccessor httpContextAccessor, ILogger<ApiHub> logger)
    {
        this._httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        Trace.TraceInformation("MapHub started. ID: {0}", Context.ConnectionId);

        var id = Context.User.Identities.First().Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        List<string> existingUserConnectionIds;
        Storage.ConnectedUsers.TryGetValue(id, out existingUserConnectionIds);

        if (existingUserConnectionIds == null)
        {
            existingUserConnectionIds = new List<string>();
        }

        existingUserConnectionIds.Add(Context.ConnectionId);

        Storage.ConnectedUsers.TryAdd(id, existingUserConnectionIds);

        return base.OnConnectedAsync();
    }


    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User.Identities.First().Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        lock (Storage.ConnectedUsers)
        {
            if (Storage.ConnectedUsers.ContainsKey(userId))
            {
                Storage.ConnectedUsers[userId].Remove(connectionId);
                if (Storage.ConnectedUsers[userId].Count == 0)
                {
                    List<string> garbage; // to be collected by the Garbage Collector
                    Storage.ConnectedUsers.TryRemove(userId, out garbage);
                }
            }
        }
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendSignal(EventsEnum eventType, string userId)
    {
        if (Storage.ConnectedUsers.TryGetValue(userId, out var ids))
        {
            await Clients.Clients(ids).SendAsync(eventType.ToString());
        }
        else
        {
            _logger.LogWarning("Пользователь с id " + userId + " не подключен к хабу");
        }
    }

    public async Task SendSignalRange(EventsEnum eventType, List<Guid> userIds)
    {
        foreach (var id in userIds)
        {
            if (Storage.ConnectedUsers.TryGetValue(id.ToString(), out var ids))
            {
                await Clients.Clients(ids).SendAsync(eventType.ToString());
            }
            else
            {
                _logger.LogWarning("Пользователь с id " + id + " не подключен к хабу");
            }
        }
    }
}
