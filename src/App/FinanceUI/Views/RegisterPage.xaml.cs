using FinanceUI.Models;

namespace FinanceUI.Views;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;
    
    // Injeção de dependência no construtor
    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Sugestão: Definir uma data máxima para o picker (ex: 18 anos atrás)
        DataNascPicker.MaximumDate = DateTime.Now.AddYears(-18);
    }

    private void AoClicarMostrarOcultarPass(object sender, EventArgs e)
    {
        // Inverte o estado atual
        PassEntry.IsPassword = !PassEntry.IsPassword;

        // Altera o ícone
        BtnTogglePass.Source = PassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private void AoClicarMostrarOcultarConfirmPass(object sender, EventArgs e)
    {
        // Inverte o estado atual
        ConfirmPassEntry.IsPassword = !ConfirmPassEntry.IsPassword;

        // Altera o ícone
        BtnToggleConfirmPass.Source = ConfirmPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private async void AoClicarRegistar(object sender, EventArgs e)
    {
        // 1. Validações básicas de UI
        if (string.IsNullOrWhiteSpace(NomeEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(TelemovelEntry.Text) ||
            string.IsNullOrWhiteSpace(PassEntry.Text))
        {
            await DisplayAlertAsync("Erro", "Por favor, preencha todos os campos.", "OK");
            return;
        }

        if (PassEntry.Text != ConfirmPassEntry.Text)
        {
            await DisplayAlertAsync("Erro", "As palavras-passe não coincidem!", "OK");
            return;
        }

        if (PassEntry.Text.Length < 6)
        {
            await DisplayAlertAsync("Erro", "A palavra-passe deve ter pelo menos 6 caracteres.", "OK");
            return;
        }

        // 2. Iniciar animação
        Indicador.IsRunning = true;
        BtnRegistar.IsEnabled = false;

        try
        {
            // Criar o objeto com os dados dos campos
            var novoRegisto = new RegistoRequestDTO
            {
                Nome = NomeEntry.Text,
                Email = EmailEntry.Text,
                Telemovel = TelemovelEntry.Text,
                DataNasc = (DateTime)DataNascPicker.Date,
                Password = PassEntry.Text
            };

            // 3. Chamada à API
            var resultado = await _apiService.RegistarAsync(novoRegisto);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso", "Conta criada com sucesso! Já pode fazer login.", "OK");

                // Navega de volta para a página anterior (Login)
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlertAsync("Erro no Registo", resultado.Mensagem, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro Crítico", ex.Message, "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
            BtnRegistar.IsEnabled = true;
        }
    }

    private async void AoClicarIrParaLogin(object sender, EventArgs e)
    {
        // Navega de volta para a página de Login
        await Navigation.PopAsync();
    }
}