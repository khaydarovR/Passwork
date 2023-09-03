using Microsoft.Extensions.Caching.Distributed;
using Passwork.Server.Application.Interfaces;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Xml.Linq;

namespace Passwork.Server.Application.Services;

public class DeferredInviterService : IDeferredInviter
{
    private readonly IDistributedCache distributedCache;
    private readonly string _prefix = "sinv:";
    
    private string GetFullKey(string lastname)
    {
        return _prefix+lastname;
    }
    public DeferredInviterService(IDistributedCache distributedCache)
    {
        this.distributedCache = distributedCache;
    }
    public async Task<List<string>> GetSafeIdsAndRemoveValues(string userEmail)
    {
        var key = GetFullKey(userEmail);
        string? jsonSafeIds = await distributedCache.GetStringAsync(key);
        distributedCache.Remove(userEmail);

        var result = JsonSerializer.Deserialize<List<string>>(jsonSafeIds);
        return result;
    }

    public async Task<bool> ValueIsExists(string userEmail)
    {
        var key = GetFullKey(userEmail);
        string? result = await distributedCache.GetStringAsync(key);
        if (string.IsNullOrEmpty(result))
        {
            return false;
        }
        return true;
    }

    public async Task WaitInvite(string userEmail, string safeId, TimeSpan saveTime)
    {
        var key = GetFullKey(userEmail);

        string? oldJsonSafeIds = await distributedCache.GetStringAsync(key);
        List<string>? oldSafeList = null;
        if (oldJsonSafeIds is not null)
        {
            oldSafeList = JsonSerializer.Deserialize<List<string>>(oldJsonSafeIds);
        }
        oldSafeList = oldSafeList ?? new List<string>();
        oldSafeList.Add(safeId);

        var newJsonSafeIds = JsonSerializer.Serialize(oldSafeList);
        var saveOptions = new DistributedCacheEntryOptions();
        saveOptions.AbsoluteExpirationRelativeToNow = saveTime;
        await distributedCache.SetStringAsync(key, newJsonSafeIds, saveOptions);
    }
}
