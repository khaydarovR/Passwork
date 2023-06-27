using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace Passwork.Server.Application.Services.SignalR;

[Authorize]
public class ApiHub: Microsoft.AspNetCore.SignalR.Hub
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiHub(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
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

    public async Task SendCompanyUpdate(string userId)
    {
        var ids = Storage.ConnectedUsers[userId];
        await Clients.Clients(ids).SendAsync("CompanyUpdated");
    }
}
