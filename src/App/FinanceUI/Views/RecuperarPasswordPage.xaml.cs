namespace FinanceUI.Views;

public partial class RecuperarPasswordPage : ContentPage
{
    private readonly ApiService _apiService;

    public RecuperarPasswordPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
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
}