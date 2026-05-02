using System.Net.Http.Json;
using System.Net.Http.Headers;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:7221/api/" : "https://localhost:7221/api/";

    public ApiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    // --- MÉTODO DE LOGIN (Já tínhamos falado) ---
    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("Auth/login", new { Email = email, Password = password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.Token;
        }
        return null;
    }

    // --- NOVO MÉTODO: ADICIONA ISTO PARA RESOLVER O ERRO ---
    public async Task<List<ContaExemplo>> GetContasAsync()
    {
        // 1. Ir buscar o token guardado no login
        var token = await SecureStorage.Default.GetAsync("auth_token");

        // 2. Adicionar o token no cabeçalho para a API te deixar entrar (Authorize)
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 3. Chamar o endpoint que criaste no ContasController
        var response = await _httpClient.GetAsync("Contas/dropdown");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<ContaExemplo>>();
        }
        return new List<ContaExemplo>();
    }
}

// Classes auxiliares para o JSON
public class TokenResponse { public string Token { get; set; } }
public class ContaExemplo { public int Id { get; set; } public string Nome { get; set; } public decimal Saldo { get; set; } }