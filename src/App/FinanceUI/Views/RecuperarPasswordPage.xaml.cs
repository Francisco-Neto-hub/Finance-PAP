using FinanceUI.Services;
using System.Text.RegularExpressions;

namespace FinanceUI.Views;

public partial class RecuperarPasswordPage : ContentPage
{
    private readonly ApiService _apiService;

    public RecuperarPasswordPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    #region Visibilidade de Passwords
    private void AoClicarMostrarOcultarNovaPass(object sender, EventArgs e)
    {
        if (NovaPassEntry == null) return;
        NovaPassEntry.IsPassword = !NovaPassEntry.IsPassword;
        BtnToggleNovaPass.Source = NovaPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private void AoClicarMostrarOcultarConfirmPass(object sender, EventArgs e)
    {
        if (ConfirmarPassEntry == null) return;
        ConfirmarPassEntry.IsPassword = !ConfirmarPassEntry.IsPassword;
        BtnToggleConfirmPass.Source = ConfirmarPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }
    #endregion

    private async void AoClicarRecuperar(object sender, EventArgs e)
    {
        // 1. Captura e Limpeza (Trim() remove espaços que o utilizador mete sem querer)
        var email = EmailEntry?.Text?.Trim() ?? string.Empty;
        var telemovel = TelemovelEntry?.Text?.Trim() ?? string.Empty;
        var novaPass = NovaPassEntry?.Text ?? string.Empty;
        var confirmPass = ConfirmarPassEntry?.Text ?? string.Empty;

        // 2. Validações de Identidade
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(telemovel))
        {
            await DisplayAlertAsync("Aviso", "Preencha o Email e o Telemóvel para confirmar a sua identidade.", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Email Inválido", "Por favor, insira um formato de email correto.", "OK");
            return;
        }

        // 3. Validações de Password
        if (string.IsNullOrWhiteSpace(novaPass) || novaPass.Length < 6)
        {
            await DisplayAlertAsync("Segurança", "A nova password deve ter pelo menos 6 caracteres.", "OK");
            return;
        }

        if (novaPass != confirmPass)
        {
            await DisplayAlertAsync("Erro", "As passwords introduzidas não são iguais.", "OK");
            return;
        }

        try
        {
            // 4. Iniciar Estado de Carregamento
            Indicador.IsRunning = true;
            BtnRecuperar.IsEnabled = false;

            // 5. Chamada à API (Lembrando: resultado é uma Tupla, não pode ser nulo)
            var resultado = await _apiService.RecuperarPasswordAsync(email, telemovel, novaPass);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso ✨", "A sua password foi redefinida com sucesso!", "OK");

                // Volta para o Login
                await Navigation.PopAsync();
            }
            else
            {
                // Mensagem vinda da API (ex: "Utilizador não encontrado")
                await DisplayAlertAsync("Erro", resultado.Mensagem, "Tentar novamente");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro de Ligação", "Não foi possível comunicar com o servidor. Verifique a sua internet.", "OK");
        }
        finally
        {
            // 6. Repor estado original (sempre executa)
            Indicador.IsRunning = false;
            BtnRecuperar.IsEnabled = true;
        }
    }

    private async void AoClicarVoltarLogin(object sender, EventArgs e)
    {
        // Garante uma navegação segura de volta
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }

    // Validação de Email (Regex)
    private bool IsValidEmail(string email)
    {
        try
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }
        catch { return false; }
    }
}