namespace FinanceUI.Views;

public partial class RecuperarPasswordPage : ContentPage
{
    private readonly ApiService _apiService;

    public RecuperarPasswordPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private void AoClicarMostrarOcultarNovaPass(object sender, EventArgs e)
    {
        // Inverte o estado atual
        NovaPassEntry.IsPassword = !NovaPassEntry.IsPassword;

        // Altera o ícone
        BtnToggleNovaPass.Source = NovaPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private void AoClicarMostrarOcultarConfirmPass(object sender, EventArgs e)
    {
        // Inverte o estado atual
        ConfirmarPassEntry.IsPassword = !ConfirmarPassEntry.IsPassword;

        // Altera o ícone
        BtnToggleConfirmPass.Source = ConfirmarPassEntry.IsPassword ? "olho_visivel.png" : "olho_oculto.png";
    }

    private async void AoClicarRecuperar(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(TelemovelEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Preencha o Email e Telemóvel para confirmar a identidade.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(NovaPassEntry.Text) || NovaPassEntry.Text != ConfirmarPassEntry.Text)
        {
            await DisplayAlertAsync("Aviso", "As passwords não coincidem ou estão vazias.", "OK");
            return;
        }

        Indicador.IsRunning = true;
        BtnRecuperar.IsEnabled = false;

        var resultado = await _apiService.RecuperarPasswordAsync(EmailEntry.Text, TelemovelEntry.Text, NovaPassEntry.Text);

        Indicador.IsRunning = false;
        BtnRecuperar.IsEnabled = true;

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Sucesso", "A sua password foi redefinida. Já pode fazer login!", "OK");
            await Navigation.PopAsync(); // Volta para a página de Login
        }
        else
        {
            await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
        }
    }

    private async void AoClicarVoltarLogin(object sender, EventArgs e)
    {
        // Navega de volta para a página de Login
        await Navigation.PopAsync();
    }
}