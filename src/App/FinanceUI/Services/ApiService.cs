using FinanceUI.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using static FinanceUI.Models.UserDTO;
using static FinanceUI.Models.GraficosDTO;
using static FinanceUI.Models.RelatoriosDTO;

namespace FinanceUI.Services; // Adicionado namespace para organização

public class ApiService
{
    private readonly HttpClient _httpClient;

    // Simplificação da URL Base
    private static string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? "https://10.0.2.2:7221/api/"
        : "https://localhost:7221/api/";

    public ApiService()
    {
        // Configuração de segurança para certificados auto-assinados (comum em dev local)
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
    }

    // --- MÉTODOS AUXILIARES (INTERNOS) ---

    /// <summary>
    /// Garante que o Token atual do SecureStorage está no cabeçalho antes de cada pedido.
    /// </summary>
    private async Task PrepararCabecalhoAuth()
    {
        var token = await SecureStorage.Default.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private string ExtrairIdDoToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "idCliente" || c.Type == "sub");
            return idClaim?.Value;
        }
        catch { return null; }
    }

    // --- AUTENTICAÇÃO ---

    public async Task<string> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", new { Email = email, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result?.Token != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                    return result.Token;
                }
            }
            return null;
        }
        catch (Exception ex) { Debug.WriteLine($"Erro Login: {ex.Message}"); return null; }
    }

    public async Task<(bool Sucesso, string Mensagem)> RegistarAsync(RegistoRequestDTO dados)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/registar", dados);
            var conteudo = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? (true, "Conta criada com sucesso!")
                : (false, conteudo);
        }
        catch (Exception ex) { return (false, $"Erro de ligação: {ex.Message}"); }
    }

    // --- UTILIZADOR E PERFIL ---

    public async Task<UserUpdateDTO> ObterPerfilAsync()
    {
        try
        {
            await PrepararCabecalhoAuth();
            return await _httpClient.GetFromJsonAsync<UserUpdateDTO>("User/perfil");
        }
        catch { return null; }
    }

    public async Task<(bool Sucesso, string Mensagem)> AtualizarPerfilAsync(string nome, string email, string telemovel, DateTime dataNasc)
    {
        try
        {
            await PrepararCabecalhoAuth();
            var dados = new { Nome = nome, Email = email, Telemovel = telemovel, DataNasc = dataNasc };
            var response = await _httpClient.PutAsJsonAsync("User/atualizar-perfil", dados);

            return response.IsSuccessStatusCode
                ? (true, "Perfil atualizado!")
                : (false, await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool Sucesso, string Mensagem)> MudarPasswordAsync(string antiga, string nova)
    {
        try
        {
            await PrepararCabecalhoAuth();
            var response = await _httpClient.PutAsJsonAsync("User/mudar-password", new { PasswordAntiga = antiga, PasswordNova = nova });

            return response.IsSuccessStatusCode
                ? (true, "Password alterada!")
                : (false, "A password antiga está incorreta.");
        }
        catch { return (false, "Erro ao mudar password."); }
    }

    // --- MÉTODO PARA A RECUPERAÇÃO DA PASSWORD ---
    public async Task<(bool Sucesso, string Mensagem)> RecuperarPasswordAsync(string email, string telemovel, string novaPassword)
    {
        try
        {
            // Nota: Aqui não chamamos PrepararCabecalhoAuth() porque o utilizador não está logado
            var dados = new { Email = email, Telemovel = telemovel, NovaPassword = novaPassword };

            var response = await _httpClient.PostAsJsonAsync("User/recuperar-password", dados);

            if (response.IsSuccessStatusCode)
                return (true, "Password recuperada com sucesso!");

            var erro = await response.Content.ReadAsStringAsync();
            return (false, erro);
        }
        catch (Exception ex)
        {
            return (false, $"Erro de rede: {ex.Message}");
        }
    }

    // --- DASHBOARD E CONTAS ---

    public async Task<DashboardResumo> GetResumoDashboardAsync()
    {
        try
        {
            await PrepararCabecalhoAuth();
            return await _httpClient.GetFromJsonAsync<DashboardResumo>("Dashboard/resumo");
        }
        catch { return null; }
    }

    public async Task<List<ContaDTO>> GetContasAsync()
    {
        try
        {
            await PrepararCabecalhoAuth();
            return await _httpClient.GetFromJsonAsync<List<ContaDTO>>("Contas/dropdown") ?? new();
        }
        catch { return new(); }
    }

    // --- MÉTODO PARA FECHAR UMA CONTA FINANCEIRA (SOFT DELETE) ---
    public async Task<(bool Sucesso, string Mensagem)> FecharContaAsync(int idConta)
    {
        try
        {
            await PrepararCabecalhoAuth();

            // Como é um PUT sem corpo (body), enviamos null ou um conteúdo vazio
            var response = await _httpClient.PutAsync($"Contas/{idConta}/fechar_conta", null);

            if (response.IsSuccessStatusCode)
                return (true, "Conta encerrada com sucesso.");

            var erro = await response.Content.ReadAsStringAsync();
            return (false, erro);
        }
        catch (Exception ex)
        {
            return (false, $"Erro de ligação: {ex.Message}");
        }
    }

    public async Task<List<CategoriaDTO>> GetCategoriasAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<CategoriaDTO>>("Categorias"); }
        catch { return new(); }
    }

    public async Task<(bool Sucesso, string Mensagem)> CriarContaAsync(string nomeConta, decimal montanteInicial)
    {
        try
        {
            await PrepararCabecalhoAuth();
            var response = await _httpClient.PostAsJsonAsync("Contas/nova_conta", new { NomeConta = nomeConta, Montante = montanteInicial });
            return response.IsSuccessStatusCode ? (true, "Sucesso") : (false, "Erro ao criar conta.");
        }
        catch { return (false, "Erro de rede."); }
    }

    // --- TRANSAÇÕES E RELATÓRIOS ---

    public async Task<bool> PostTransacaoAsync(TransacaoRequestDTO transacao)
    {
        try
        {
            await PrepararCabecalhoAuth();
            var response = await _httpClient.PostAsJsonAsync("Transacoes", transacao);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<TransacaoReadDTO>> ObterExtratoAsync(int idConta, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        await PrepararCabecalhoAuth();
        string url = $"Transacoes/extrato/{idConta}";
        var queries = new List<string>();

        if (dataInicio.HasValue) queries.Add($"dataInicio={dataInicio.Value:yyyy-MM-dd}");
        if (dataFim.HasValue) queries.Add($"dataFim={dataFim.Value:yyyy-MM-dd}");

        if (queries.Any()) url += "?" + string.Join("&", queries);

        return await _httpClient.GetFromJsonAsync<List<TransacaoReadDTO>>(url) ?? new();
    }

    public async Task<List<RelatorioTransacaoDTO>> GetExtratoDetalhadoAsync(int mes, int ano, int? idConta, int? idCategoria, string busca)
    {
        await PrepararCabecalhoAuth();
        var url = $"Relatorios/extrato-detalhado?mes={mes}&ano={ano}";
        if (idConta > 0) url += $"&idConta={idConta}";
        if (idCategoria > 0) url += $"&idCategoria={idCategoria}";
        if (!string.IsNullOrEmpty(busca)) url += $"&busca={Uri.EscapeDataString(busca)}";

        return await _httpClient.GetFromJsonAsync<List<RelatorioTransacaoDTO>>(url) ?? new();
    }

    // --- GRÁFICOS ---

    public async Task<List<GastoCategoriaDTO>> GetGastosPorCategoriaAsync(int mes, int ano, int? idConta)
    {
        await PrepararCabecalhoAuth();
        var url = $"Graficos/despesas-categoria?mes={mes}&ano={ano}";
        if (idConta > 0) url += $"&idConta={idConta}";
        return await _httpClient.GetFromJsonAsync<List<GastoCategoriaDTO>>(url) ?? new();
    }

    public async Task<List<FluxoCaixaDTO>> GetFluxoCaixaAsync(int ano, int? idConta)
    {
        await PrepararCabecalhoAuth();
        var url = $"Graficos/fluxo-caixa?ano={ano}";
        if (idConta > 0) url += $"&idConta={idConta}";
        return await _httpClient.GetFromJsonAsync<List<FluxoCaixaDTO>>(url) ?? new();
    }

    // --- SUPORTE ---

    public async Task<(bool Sucesso, string Mensagem)> EnviarTicketAsync(string assunto, string mensagem)
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("auth_token");
            var userId = ExtrairIdDoToken(token);

            if (string.IsNullOrEmpty(userId)) return (false, "Utilizador não identificado.");

            await PrepararCabecalhoAuth();
            var response = await _httpClient.PostAsJsonAsync($"User/{userId}/enviar-ticket", new { Assunto = assunto, Mensagem = mensagem });

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<RespostasSuporte>();
                return (true, res?.mensagem ?? "Ticket enviado!");
            }
            return (false, "Falha ao enviar ticket.");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}

// Modelos de Resposta Rápidos
public class TokenResponse { public string Token { get; set; } }
public class RespostasSuporte { public string mensagem { get; set; } }