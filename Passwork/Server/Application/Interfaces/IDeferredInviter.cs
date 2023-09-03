namespace Passwork.Server.Application.Interfaces
{
    public interface IDeferredInviter
    {
        Task<List<string>> GetSafeIdsAndRemoveValues(string userEmail);
        Task<bool> ValueIsExists(string userEmail);
        Task WaitInvite(string userEmail, string safeId, TimeSpan saveTime);
    }
}