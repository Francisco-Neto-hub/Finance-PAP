namespace FinanceUI;

public partial class LoginPage : ContentPage
{
    // Criamos uma variável privada para guardar o serviço
    private readonly ApiService _apiService;

    // O MAUI injeta o ApiService aqui porque o registaste no MauiProgram
    public LoginPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void AoClicarEntrar(object sender, EventArgs e)
    {
        // 1. Validação básica
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PassEntry.Text))
        {
            await DisplayAlert("Aviso", "Por favor, preencha as credenciais.", "OK");
            return;
        }

        try
        {
            // 2. Iniciar estado de carregamento
            Indicador.IsRunning = true;
            BtnLogin.IsEnabled = false;

            // 3. Chamar o serviço (Método que criamos anteriormente no ApiService)
            // O LoginAsync deve devolver o Token JWT se for bem sucedido
            string token = await _apiService.LoginAsync(EmailEntry.Text, PassEntry.Text);

            if (!string.IsNullOrEmpty(token))
            {
                // 4. Guardar o token de forma segura no telemóvel
                await SecureStorage.Default.SetAsync("auth_token", token);

                // Dentro da LoginPage.xaml.cs, após validar o login na API:
                await Navigation.PushAsync(new MainPage());
                // OU, se quiseres usar o Menu Lateral (Shell):
                Application.Current.MainPage = new AppShell();

            }
            else
            {
                await DisplayAlert("Erro", "Email ou palavra-passe incorretos.", "Tentar novamente");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Ligação", "Não foi possível ligar ao servidor. Verifique a internet ou se a API está ligada.", "OK");
        }
        finally
        {
            // 6. Repor estado do botão
            Indicador.IsRunning = false;
            BtnLogin.IsEnabled = true;
        }
    }
    private async void AoClicarCriarConta(object sender, EventArgs e)
    {
        // Navega para a página de registo
        // Usamos PushAsync para que o utilizador possa voltar atrás para o Login se quiser
        await Navigation.PushAsync(new RegisterPage());
    }
}