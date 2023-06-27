using System.Collections.Concurrent;

namespace Passwork.Server.Application.Services.SignalR;

public static class Storage
{
    public static ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();
}
