using Blazored.LocalStorage;
using System.Globalization;

namespace Passwork.ClientModule.Services;

public class TokenService
{
    private const string _key = "token";
    private readonly ILocalStorageService _localStorageService;

    public TokenService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    /// <summary>
    /// Удалить токен из локального хранилища.
    /// </summary>
    public async Task DeleteTokenAsync()
    {
        await _localStorageService.RemoveItemAsync(_key);
    }

    /// <summary>
    /// Получить токен из локального хранилища.
    /// </summary>
    /// <returns><see cref="Task{TResult}">Task&lt;string&gt;</see></returns>
    public async Task<string> GetTokenAsync() =>
        await _localStorageService.GetItemAsync<string>(_key);

    /// <summary>
    /// Установить токен в локальное хранилище.
    /// </summary>
    /// <param name="token">Токен.</param>
    public void SetToken(string token)
    {
        if (token != null)
        {
            Task.FromResult(_localStorageService.SetItemAsync<string>(_key, token));
        }
    }
}
