using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Passwork.Shared.ViewModels;
using System.Net.Http.Json;

namespace Passwork.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly HubConnection _hubConnection;
    public event Action OnCompanyUpdated;

    public List<CompaniesOwnerVm> Companies { get; set; } = new();
    public List<CompaniesOwnerVm> OwnerCompanies { get; set; } = new();

    public ApiService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
    }

    public async Task<bool> PostDataAsync<T>(string url, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        
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
            OwnerCompanies = await response.Content.ReadFromJsonAsync<List<CompaniesOwnerVm>>();
            //Snackbar.Add("Получены ваши компании");
        }
        else
        {
            //Snackbar.Add("Ошибка");
        }
    }

    public async Task LoadLinkedCom()
    {
        var response = await _httpClient.GetAsync("/api/Company/GetAll");

        if (response.IsSuccessStatusCode)
        {
            Companies = await response.Content.ReadFromJsonAsync<List<CompaniesOwnerVm>>();
            //Snackbar.Add("Получены связанные компании");
        }
        else
        {
            //Snackbar.Add("Ошибка");
        }
    }
}