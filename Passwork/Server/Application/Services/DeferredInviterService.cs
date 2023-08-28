using Microsoft.Extensions.Caching.Distributed;
using Passwork.Server.Application.Interfaces;
using System.Diagnostics.Contracts;
using System.Xml.Linq;

namespace Passwork.Server.Application.Services;

public class DeferredInviterService : IDeferredInviter
{
    private readonly IDistributedCache distributedCache;

    public DeferredInviterService(IDistributedCache distributedCache)
    {
        this.distributedCache = distributedCache;
    }
    public async Task<string> GetSafeAndRemoveValue(string userEmail)
    {
        string? result = await distributedCache.GetStringAsync(userEmail);
        distributedCache.Remove(userEmail);
        return result;
    }

    public async Task<bool> ValueIsExists(string userEmail)
    {
        string? result = await distributedCache.GetStringAsync(userEmail);
        if (string.IsNullOrWhiteSpace(result))
        {
            return false;
        }
        return true;
    }

    public async Task WaitInvite(string userEmail, string safeId, TimeSpan saveTime)
    {
        var saveOptions = new DistributedCacheEntryOptions();
        saveOptions.AbsoluteExpirationRelativeToNow = saveTime;

        await distributedCache.SetStringAsync(userEmail, safeId, saveOptions);
    }
}
