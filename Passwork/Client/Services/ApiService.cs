using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Passwork.Shared.ViewModels;
using System.Net.Http.Json;

namespace Passwork.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    public event Action OnCompanyUpdated;
    public ErrorMessage CurrentMessage = new();
    public List<CompaniesVm> Companies { get; set; } = null!;
    public List<CompaniesVm> OwnerCompanies { get; set; } = null!;
    public List<TagVm> Tags { get; set; } = new();
    public List<PasswordVm> Passwords { get; set; } = new();
    public PasswordDetailVm PasswordDetail { get; set; } = new();
    public List<SafeUserVm> SafeUsers { get; private set; }
    public List<ComUserVm> ComUsers { get; private set; }

    public ApiService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    public async Task<bool> PostDataAsync<T>(string url, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        if(response.IsSuccessStatusCode == false)
        {
            CurrentMessage = await response.Content.ReadFromJsonAsync<ErrorMessage>()?? new ErrorMessage { Message = "Ошибка"};
        }
        return response.IsSuccessStatusCode;
    }


    public void NavigateTo(string url)
    {
        _navigationManager.NavigateTo(url);
    }

    public async Task LoadOwnerCom()
    {
        var response = await _httpClient.GetAsync("/api/Company/OwnerCom");
        if (response.IsSuccessStatusCode)
        {
            OwnerCompanies = new();
            OwnerCompanies = await response.Content.ReadFromJsonAsync<List<CompaniesVm>>()?? new List<CompaniesVm>();
        }
    }

    public async Task<string> LoadLinkedCom()
    {
        var response = await _httpClient.GetAsync("/api/Company/GetAll");

        if (response.IsSuccessStatusCode)
        {
            Companies = await response.Content.ReadFromJsonAsync<List<CompaniesVm>>()?? new List<CompaniesVm>();
            return null;
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>() ?? new ErrorMessage() { Message = "Не известная ошибка" };
            return error.Message;
        }
    }

    public async Task LoadLinkedTags(Guid safeId)
    {
        var response = await _httpClient.GetAsync($"/api/Tag/Get?safeId={safeId}");

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<List<TagVm>>() ?? new List<TagVm>();
            Tags = res;
        }
    }

    public async Task<bool> LoadLinkedPasswords(Guid safeId)
    {
        var response = await _httpClient.GetAsync($"/api/Password/GetAll?safeId={safeId}");

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<List<PasswordVm>>()?? new List<PasswordVm>();
            Passwords = res;
            return true;
        }
        else
        {
            CurrentMessage = await response.Content.ReadFromJsonAsync<ErrorMessage>() ?? new ErrorMessage();
            return false;
        }
    }

    public async Task<string> LoadPasswordDetail(Guid pwId)
    {
        var response = await _httpClient.GetAsync($"/api/Password/Detail?pwId={pwId}");

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<PasswordDetailVm>() ?? new PasswordDetailVm();
            PasswordDetail = res;
            return null;
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>() ?? new ErrorMessage() { Message = "Не известная ошибка"};
            return error.Message;
        }
    }

    public async Task<string> LoadSafeUsers(Guid safeId)
    {
        var response = await _httpClient.GetAsync($"/api/Safe/Users?safeId={safeId}");

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<List<SafeUserVm>>() ?? new List<SafeUserVm>();
            SafeUsers = new();
            SafeUsers = res;
            return null;
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>() ?? new ErrorMessage() { Message = "Не известная ошибка" };
            return error.Message;
        }
    }

    public async Task<string> LoadComUsers(Guid safeId)
    {
        var response = await _httpClient.GetAsync($"/api/Company/Users?safeId={safeId}");

        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<List<ComUserVm>>() ?? new List<ComUserVm>();
            ComUsers = res;
            return null;
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorMessage>() ?? new ErrorMessage() { Message = "Не известная ошибка" };
            return error.Message;
        }
    }


    public async Task<string> LoadConnectionString(Guid safeId)
    {
        var res = await _httpClient.GetAsync($"/api/Safe/constring?safeId={safeId}");
        return res.IsSuccessStatusCode ? await res.Content.ReadAsStringAsync() : "Ошибка";
    }
}