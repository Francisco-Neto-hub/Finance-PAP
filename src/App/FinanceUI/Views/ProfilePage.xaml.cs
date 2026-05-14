using FinanceUI.Models;
using FinanceUI.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FinanceUI.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    public ProfilePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosDoUtilizador();
    }

    private async Task CarregarDadosDoUtilizador()
    {
        if (_isBusy) return;

        try
        {
            _isBusy = true;
            Indicador.IsRunning = true;

            var dados = await _apiService.ObterPerfilAsync();

            if (dados != null)
            {
                NomeEntry.Text = dados.Nome;
                EmailEntry.Text = dados.Email;
                TelemovelEntry.Text = dados.Telemovel;
                DataNascPicker.Date = dados.DataNasc;
            }
            else
            {
                await DisplayAlertAsync("Erro", "Não foi possível carregar os teus dados de perfil.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Profile Error]: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
            Indicador.IsRunning = false;
        }
    }

    #region Visibilidade de Passwords
    private void AoClicarMostrarOcultarAntigaPassword(object sender, EventArgs e)
    {
        if (AntigaPassEntry == null) return;
        AntigaPassEntry.IsPassword = !AntigaPassEntry.IsPassword;
        BtnToggleAntigaPass.Source = AntigaPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private void AoClicarMostrarOcultarNovaPassword(object sender, EventArgs e)
    {
        if (NovaPassEntry == null) return;
        NovaPassEntry.IsPassword = !NovaPassEntry.IsPassword;
        BtnToggleNovaPass.Source = NovaPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }
    #endregion

    private async void AoClicarLogout(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Sair", "Tem a certeza que deseja encerrar a sessão?", "Sim", "Não");

        if (confirmar)
        {
            try
            {
                SecureStorage.Default.Remove("auth_token");

                // Redireciona para o Login limpando a stack de navegação
                var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
            catch (Exception)
            {
                // Fallback de segurança
                Application.Current.MainPage = new NavigationPage(new LoginPage(_apiService));
            }
        }
    }

    // --- ATUALIZAR DADOS PESSOAIS ---
    private async void AoClicarGuardarPerfil(object sender, EventArgs e)
    {
        var nome = NomeEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var telemovel = TelemovelEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(telemovel))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha todos os campos pessoais.", "OK");
            return;
        }

        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync("Email Inválido", "Insira um endereço de email válido.", "OK");
            return;
        }

        try
        {
            Indicador.IsRunning = true;
            var resultado = await _apiService.AtualizarPerfilAsync(nome, email, telemovel, (DateTime)DataNascPicker.Date);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso ✨", "Perfil atualizado com sucesso!", "OK");
            }
            else
            {
                await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro Crítico", "Falha ao comunicar com o servidor.", "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
        }
    }

    // --- MUDAR PASSWORD ---
    private async void AoClicarMudarPass(object sender, EventArgs e)
    {
        var antiga = AntigaPassEntry.Text;
        var nova = NovaPassEntry.Text;

        if (string.IsNullOrWhiteSpace(antiga) || string.IsNullOrWhiteSpace(nova))
        {
            await DisplayAlertAsync("Aviso", "Preencha as duas passwords para proceder à alteração.", "OK");
            return;
        }

        if (nova.Length < 6)
        {
            await DisplayAlertAsync("Segurança", "A nova password deve ter pelo menos 6 caracteres.", "OK");
            return;
        }

        try
        {
            Indicador.IsRunning = true;
            var resultado = await _apiService.MudarPasswordAsync(antiga, nova);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso", "Password alterada com sucesso!", "OK");
                AntigaPassEntry.Text = string.Empty;
                NovaPassEntry.Text = string.Empty;
            }
            else
            {
                await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Não foi possível mudar a password.", "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
        }
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }
}