using System.Net.Http; // Para capturar erros específicos de rede
using FinanceUI.Services;
using Microsoft.Maui.Storage; // Para o SecureStorage

namespace FinanceUI.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void AoClicarEntrar(object sender, EventArgs e)
    {
        // Limpa espaços em branco acidentais no email e garante que não são null
        string email = EmailEntry?.Text?.Trim() ?? string.Empty;
        string password = PassEntry?.Text ?? string.Empty;

        // 1. Validação de campos vazios
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha o email e a palavra-passe.", "OK");
            return;
        }

        // 2. Validação do formato do Email
        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Aviso", "Por favor, insira um endereço de email válido.", "OK");
            return;
        }

        try
        {
            // 3. Iniciar estado de carregamento (Bloquear botão para evitar duplo clique)
            Indicador.IsRunning = true;
            BtnLogin.IsEnabled = false;

            // 4. Chamar o serviço
            string token = await _apiService.LoginAsync(email, password);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // 5. Guardar o token de forma segura com proteção
                    await SecureStorage.Default.SetAsync("auth_token", token);
                }
                catch (Exception)
                {
                    // Previne crash se o telemóvel tiver problemas com o Keystore/Keychain
                    await DisplayAlertAsync("Atenção", "Login feito com sucesso, mas ocorreu um erro ao guardar a sessão no dispositivo.", "OK");
                }

                // 6. Transição correta para a App Principal (Dashboard)
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlertAsync("Erro", "Email ou palavra-passe incorretos.", "Tentar novamente");
            }
        }
        catch (HttpRequestException)
        {
            // Erro específico de quando a API está desligada ou sem internet
            await DisplayAlertAsync("Erro de Ligação", "Não foi possível ligar ao servidor. Verifique a internet ou tente mais tarde.", "OK");
        }
        catch (Exception ex)
        {
            // Erro genérico (previne que a app feche abruptamente)
            await DisplayAlertAsync("Erro Inesperado", $"Ocorreu um erro: {ex.Message}", "OK");
        }
        finally
        {
            // 7. Repor estado do botão (executa sempre, mesmo que dê erro)
            Indicador.IsRunning = false;
            BtnLogin.IsEnabled = true;
        }
    }

    private async void AoClicarEsqueciPassword(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecuperarPasswordPage(_apiService));
    }

    private void AoClicarMostrarOcultarPassword(object sender, EventArgs e)
    {
        // Proteção extra: garantir que os elementos já foram renderizados antes de alterar
        if (PassEntry == null || BtnTogglePassword == null) return;

        PassEntry.IsPassword = !PassEntry.IsPassword;
        BtnTogglePassword.Source = PassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private async void AoClicarCriarConta(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_apiService));
    }

    // --- MÉTODOS AUXILIARES ---

    /// <summary>
    /// Valida rapidamente se a string tem formato de email usando recursos nativos do .NET
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}