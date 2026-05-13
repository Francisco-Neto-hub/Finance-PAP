namespace FinanceUI.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService; // Adicione este campo
    private readonly MainPage _mainPage;

    // Você deve pedir os DOIS no construtor
    public LoginPage(ApiService apiService, MainPage mainPage)
    {
        InitializeComponent();

        // Agora você guarda as duas instâncias que o .NET te deu
        _apiService = apiService;
        _mainPage = mainPage;
    }

    private async void AoClicarEntrar(object sender, EventArgs e)
    {
        // 1. Valida��o b�sica
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PassEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha as credenciais.", "OK");
            return;
        }

        try
        {
            // 2. Iniciar estado de carregamento
            Indicador.IsRunning = true;
            BtnLogin.IsEnabled = false;

            // 3. Chamar o servi�o (M�todo que criamos anteriormente no ApiService)
            // O LoginAsync deve devolver o Token JWT se for bem sucedido
            string token = await _apiService.LoginAsync(EmailEntry.Text, PassEntry.Text);

            if (!string.IsNullOrEmpty(token))
            {
                // 4. Guardar o token de forma segura no telem�vel
                await SecureStorage.Default.SetAsync("auth_token", token);

                // Dentro da LoginPage.xaml.cs, ap�s validar o login na API:
                await Navigation.PushAsync(_mainPage);                
                // OU, se quiseres usar o Menu Lateral (Shell):
                Application.Current.MainPage = new AppShell();

            }
            else
            {
                await DisplayAlertAsync("Erro", "Email ou palavra-passe incorretos.", "Tentar novamente");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro de Liga��o", "N�o foi poss�vel ligar ao servidor. Verifique a internet ou se a API est� ligada.", "OK");
        }
        finally
        {
            // 6. Repor estado do bot�o
            Indicador.IsRunning = false;
            BtnLogin.IsEnabled = true;
        }
    }

    private async void AoClicarEsqueciPassword(object sender, EventArgs e)
    {
        // Navega para a página de recuperação passando o ApiService
        await Navigation.PushAsync(new RecuperarPasswordPage(_apiService));
    }

    private void AoClicarMostrarOcultarPassword(object sender, EventArgs e)
    {
        // Inverte o estado atual do campo de palavra-passe
        PassEntry.IsPassword = !PassEntry.IsPassword;

        // Altera o ícone do botão com base na visibilidade
        if (PassEntry.IsPassword)
        {
            BtnTogglePassword.Source = "olho_visivel.png";
        }
        else
        {
            BtnTogglePassword.Source = "olho_oculto.png";
        }
    }
    private async void AoClicarCriarConta(object sender, EventArgs e)
    {
        // Navega para a p�gina de registo
        // Usamos PushAsync para que o utilizador possa voltar atr�s para o Login se quiser
        await Navigation.PushAsync(new RegisterPage(_apiService));
    }
}