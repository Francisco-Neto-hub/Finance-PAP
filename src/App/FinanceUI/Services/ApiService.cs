using FinanceUI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
            var token = result?.Token;

            if (!string.IsNullOrEmpty(token))
            {
                // CONFIGURA O TOKEN NO HTTPCLIENT AQUI!
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return token;
        }
        return null;
    }

    // Adiciona estes dois métodos à tua classe ApiService
    public async Task<(bool Sucesso, string Mensagem)> MudarPasswordAsync(string antiga, string nova)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dados = new { PasswordAntiga = antiga, PasswordNova = nova };

            // CORREÇÃO: Mudámos de PostAsJsonAsync para PutAsJsonAsync
            var response = await _httpClient.PutAsJsonAsync("Auth/mudar-password", dados);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Password alterada com sucesso!");
            }
            else
            {
                string erroDaApi = await response.Content.ReadAsStringAsync();
                return (false, $"A API recusou ({response.StatusCode}): {erroDaApi}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Erro de ligação: {ex.Message}");
        }
    }

    public async Task<bool> RecuperarPasswordAsync(string email, string telemovel, string novaPass)
    {
        try
        {
            var dados = new { Email = email, Telemovel = telemovel, NovaPassword = novaPass };

            // CORREÇÃO AQUI TAMBÉM (se a tua API usar PUT para recuperar)
            // Se a recuperação continuar a dar 405 com o PUT, volta a meter PostAsJsonAsync neste método específico.
            var response = await _httpClient.PutAsJsonAsync("Auth/recuperar-password", dados);

            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
    public async Task<DashboardResumo> GetResumoDashboardAsync()
    {
        try
        {
            // 1. Fazemos a chamada manualmente para verificar o Status Code
            var response = await _httpClient.GetAsync("Dashboard/resumo");

            // 2. Se a API responder com sucesso (200 OK)
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DashboardResumo>();
            }

            // 3. Se chegar aqui, a API deu erro (ex: 401 Unauthorized ou 404)
            var conteudoErro = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($" Erro da API: {response.StatusCode} - {conteudoErro}");
            return null;
        }
        catch (Exception ex)
        {
            // 4. Erro de rede ou JSON mal formado
            Debug.WriteLine($" Exceção no GetResumo: {ex.Message}");
            return null;
        }
    }
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

    public async Task<bool> PostTransacaoAsync(TransacaoRequest transacao)
    {
        try
        {
            // 1. Ir buscar o token que guardámos no Login
            var token = await SecureStorage.Default.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token)) return false;

            // 2. Configurar o cabeçalho de autorização
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3. Serializar o objeto para JSON
            string json = JsonSerializer.Serialize(transacao);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            // 4. Fazer o pedido POST sem a barra inicial "/"
            // Assim ele junta corretamente: .../api/ + Transacoes
            var response = await _httpClient.PostAsync("Transacoes", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // BÓNUS: Se a API der erro, lemos a mensagem de erro da API para o Output (Debug)
                var erroApi = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[Erro API - {response.StatusCode}]: {erroApi}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro crítico ao registar transação: {ex.Message}");
            return false;
        }
    }
}

// Classes auxiliares para o JSON
public class TokenResponse { public required string Token { get; set; } }
public class ContaExemplo { public int Id { get; set; } public required string Nome { get; set; } public decimal Saldo { get; set; } }