using FinanceUI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Finance.API.DTOs.UserDTO;

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

    public async Task<(bool Sucesso, string Mensagem)> RegistarAsync(RegistoRequestDTO dados)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/registar", dados);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Conta criada com sucesso!");
            }
            else
            {
                // Tenta ler a mensagem de erro da API (ex: "Este email já está registado")
                var conteudo = await response.Content.ReadAsStringAsync();
                // Se a API devolve um objeto JSON { "mensagem": "..." }, podes extrair aqui
                return (false, conteudo);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Erro de ligação: {ex.Message}");
        }
    }

    public async Task<(bool Sucesso, string Mensagem)> AtualizarPerfilAsync(string nome, string email, string telemovel, DateTime dataNasc)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dados = new { Nome = nome, Email = email, Telemovel = telemovel, DataNasc = dataNasc };
            var response = await _httpClient.PutAsJsonAsync("User/atualizar-perfil", dados);

            if (response.IsSuccessStatusCode) return (true, "Perfil atualizado!");

            var erro = await response.Content.ReadAsStringAsync();
            return (false, erro);
        }
        catch (Exception ex) { return (false, ex.Message); }
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
            var response = await _httpClient.PutAsJsonAsync("User/mudar-password", dados);

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

    public async Task<(bool Sucesso, string Mensagem)> RecuperarPasswordAsync(string email, string telemovel, string novaPassword)
    {
        try
        {
            var dados = new { Email = email, Telemovel = telemovel, NovaPassword = novaPassword };

            // Aqui não passamos Token, porque o utilizador não está logado
            var response = await _httpClient.PostAsJsonAsync("User/recuperar-password", dados);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Password recuperada com sucesso!");
            }

            var erro = await response.Content.ReadAsStringAsync();
            return (false, erro);
        }
        catch (Exception ex)
        {
            return (false, $"Erro: {ex.Message}");
        }
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

    // 1. LISTAR CONTAS
    public async Task<List<ContaDTO>> GetContasAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("Contas/dropdown");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ContaDTO>>() ?? new List<ContaDTO>();
            }
            return new List<ContaDTO>();
        }
        catch { return new List<ContaDTO>(); }
    }

    public async Task<UserUpdateDTO> ObterPerfilAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("User/perfil");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserUpdateDTO>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao buscar perfil: {ex.Message}");
            return null;
        }
    }

    public async Task<List<TransacaoReadDTO>> ObterExtratoAsync(int idConta, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = $"Transacoes/extrato/{idConta}"; 
        var parametros = new List<string>();

        if (dataInicio.HasValue)
            parametros.Add($"dataInicio={dataInicio.Value:yyyy-MM-dd}");

        if (dataFim.HasValue)
            parametros.Add($"dataFim={dataFim.Value:yyyy-MM-dd}");

        if (parametros.Any())
            url += "?" + string.Join("&", parametros);

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var extrato = await response.Content.ReadFromJsonAsync<List<TransacaoReadDTO>>();
            return extrato ?? new List<TransacaoReadDTO>();
        }
        else
        {
            // AQUI ESTÁ A MAGIA: Lemos o erro da API e "atiramos" para a página ler!
            string erroDaApi = await response.Content.ReadAsStringAsync();
            throw new Exception($"API devolveu {response.StatusCode}: {erroDaApi}");
        }
    }

    // CRIAR CONTA
    public async Task<(bool Sucesso, string Mensagem)> CriarContaAsync(string nomeConta, decimal montanteInicial)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dados = new ContaUserCreateDTO { NomeConta = nomeConta, Montante = montanteInicial };
            var response = await _httpClient.PostAsJsonAsync("Contas/nova_conta", dados);

            if (response.IsSuccessStatusCode) return (true, "Conta criada com sucesso!");

            return (false, await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // 3. FECHAR CONTA
    public async Task<(bool Sucesso, string Mensagem)> FecharContaAsync(int idConta)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Como é um PUT sem corpo (body), enviamos null
            var response = await _httpClient.PutAsync($"Contas/{idConta}/fechar_conta", null);

            if (response.IsSuccessStatusCode) return (true, "Conta encerrada.");

            return (false, await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<bool> PostTransacaoAsync(TransacaoRequestDTO transacao)
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