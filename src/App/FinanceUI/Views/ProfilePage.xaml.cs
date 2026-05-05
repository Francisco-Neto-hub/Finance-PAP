namespace FinanceUI.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    
    // Lembra-te de injetar o ApiService no construtor (e registar no MauiProgram)
    public ProfilePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }
    private async void AoClicarLogout(object sender, EventArgs e)
    {
        SecureStorage.Default.Remove("auth_token");
        var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
        Application.Current.MainPage = new NavigationPage(loginPage);
    }

    // LÓGICA PARA MUDAR PASSWORD
    private async void AoClicarMudarPass(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AntigaPassEntry.Text) || string.IsNullOrWhiteSpace(NovaPassEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Preencha ambas as passwords.", "OK");
            return;
        }

        Indicador.IsRunning = true;

        // Agora recebemos as duas partes: sucesso e a mensagem
        var resultado = await _apiService.MudarPasswordAsync(AntigaPassEntry.Text, NovaPassEntry.Text);

        Indicador.IsRunning = false;

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
            AntigaPassEntry.Text = NovaPassEntry.Text = string.Empty;
        }
        else
        {
            // Vai mostrar EXATAMENTE o que falhou
            await DisplayAlertAsync("Erro detalhado", resultado.Mensagem, "OK");
        }
    }

    // LÓGICA PARA RECUPERAR PASSWORD
    private async void AoClicarRecuperarPass(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RecuperarEmailEntry.Text) ||
            string.IsNullOrWhiteSpace(RecuperarTelEntry.Text) ||
            string.IsNullOrWhiteSpace(RecuperarNovaPassEntry.Text))
        {
            await DisplayAlertAsync("Erro", "Preencha todos os campos de recuperação.", "OK");
            return;
        }

        Indicador.IsRunning = true;
        bool sucesso = await _apiService.RecuperarPasswordAsync(
            RecuperarEmailEntry.Text,
            RecuperarTelEntry.Text,
            RecuperarNovaPassEntry.Text);
        Indicador.IsRunning = false;

        if (sucesso)
        {
            await DisplayAlertAsync("Sucesso", "Password recuperada! Já pode usar a nova.", "OK");
            RecuperarEmailEntry.Text = RecuperarTelEntry.Text = RecuperarNovaPassEntry.Text = string.Empty;
        }
        else
        {
            await DisplayAlertAsync("Erro", "Dados de recuperação inválidos.", "OK");
        }
    }
}