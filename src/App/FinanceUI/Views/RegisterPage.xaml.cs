using FinanceUI.Models;
using FinanceUI.Services;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace FinanceUI.Views;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Regra de negócio: Utilizador deve ter pelo menos 16 ou 18 anos
        DataNascPicker.MaximumDate = DateTime.Now.AddYears(-16);
        DataNascPicker.Date = DateTime.Now.AddYears(-25); // Sugestão inicial
    }

    #region Visibilidade de Password
    private void AoClicarMostrarOcultarPass(object sender, EventArgs e)
    {
        if (PassEntry == null) return;
        PassEntry.IsPassword = !PassEntry.IsPassword;
        BtnTogglePass.Source = PassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private void AoClicarMostrarOcultarConfirmPass(object sender, EventArgs e)
    {
        if (ConfirmPassEntry == null) return;
        ConfirmPassEntry.IsPassword = !ConfirmPassEntry.IsPassword;
        BtnToggleConfirmPass.Source = ConfirmPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }
    #endregion

    private async void AoClicarRegistar(object sender, EventArgs e)
    {
        // 1. Captura e Limpeza de Dados
        var nome = NomeEntry?.Text?.Trim() ?? string.Empty;
        var email = EmailEntry?.Text?.Trim() ?? string.Empty;
        var telemovel = TelemovelEntry?.Text?.Trim() ?? string.Empty;
        var pass = PassEntry?.Text ?? string.Empty;
        var confirmPass = ConfirmPassEntry?.Text ?? string.Empty;

        // 2. Validações de UI
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(telemovel) || string.IsNullOrWhiteSpace(pass))
        {
            await DisplayAlertAsync("Erro", "Todos os campos são obrigatórios.", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Email Inválido", "Por favor, insira um email válido.", "OK");
            return;
        }

        if (pass != confirmPass)
        {
            await DisplayAlertAsync("Erro", "As palavras-passe não coincidem!", "OK");
            return;
        }

        Indicador.IsRunning = true;
        BtnRegistar.IsEnabled = false;

        try
        {
            var novoRegisto = new RegistoRequestDTO
            {
                Nome = nome,
                Email = email,
                Telemovel = telemovel,
                DataNasc = DataNascPicker.Date,
                Password = pass
            };

            // 3. Chamada à API
            // 'resultado' é uma Tupla (ValueTuple), por isso não pode ser null
            var resultado = await _apiService.RegistarAsync(novoRegisto);

            // CORREÇÃO TUPLA: Acedemos diretamente às propriedades da tupla
            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso ✨", "Conta criada! Agora já pode entrar.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // CORREÇÃO: Não usamos '?' porque tuplas não são nulas
                await DisplayAlertAsync("Erro no Registo", resultado.Mensagem, "Tentar novamente");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro Crítico", "Não foi possível ligar ao servidor.", "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
            BtnRegistar.IsEnabled = true;
        }
    }

    private async void AoClicarIrParaLogin(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Validação de Email Robusta
    private bool IsValidEmail(string email)
    {
        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }
        catch { return false; }
    }
}