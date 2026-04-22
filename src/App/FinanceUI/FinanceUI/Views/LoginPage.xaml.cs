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
        var email = EmailEntry.Text;
        var pass = PassEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            await DisplayAlert("Aviso", "Preencha os dados!", "OK");
            return;
        }

        Indicador.IsRunning = true;

        // Instanciamos o serviço (mais tarde podes usar Injeção de Dependência no MauiProgram.cs)
        var api = new ApiService();
        var token = await api.LoginAsync(email, pass);

        Indicador.IsRunning = false;

        if (!string.IsNullOrEmpty(token))
        {
            // Guardar o token de forma segura (essencial para a nota da PAP!)
            await SecureStorage.Default.SetAsync("auth_token", token);

            // Navega para a Shell principal que já tens definida
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Erro", "E-mail ou Password incorretos.", "OK");
        }
    }
}