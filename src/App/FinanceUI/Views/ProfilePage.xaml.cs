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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosDoUtilizador();
    }

    private async Task CarregarDadosDoUtilizador()
    {
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
            await DisplayAlertAsync("Erro", "Não foi possível carregar os teus dados.", "OK");
        }

        Indicador.IsRunning = false;
    }

    private async void AoClicarLogout(object sender, EventArgs e)
    {
        SecureStorage.Default.Remove("auth_token");
        var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
        Application.Current.MainPage = new NavigationPage(loginPage);
    }

    // ----------------------------------------------------
    // MÉTODO: ATUALIZAR DADOS PESSOAIS
    // ----------------------------------------------------
    private async void AoClicarGuardarPerfil(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NomeEntry.Text) || 
            string.IsNullOrWhiteSpace(EmailEntry.Text) || 
            string.IsNullOrWhiteSpace(TelemovelEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Por favor, preencha todos os campos pessoais.", "OK");
            return;
        }

        Indicador.IsRunning = true;

        var resultado = await _apiService.AtualizarPerfilAsync(
            NomeEntry.Text, 
            EmailEntry.Text, 
            TelemovelEntry.Text,
            (DateTime)DataNascPicker.Date);

        Indicador.IsRunning = false;

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
        }
        else
        {
            await DisplayAlertAsync("Erro ao Guardar", resultado.Mensagem, "OK");
        }
    }

    // ----------------------------------------------------
    // MÉTODO: MUDAR PASSWORD
    // ----------------------------------------------------
    private async void AoClicarMudarPass(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AntigaPassEntry.Text) || string.IsNullOrWhiteSpace(NovaPassEntry.Text))
        {
            await DisplayAlertAsync("Aviso", "Preencha ambas as passwords.", "OK");
            return;
        }

        Indicador.IsRunning = true;
        
        var resultado = await _apiService.MudarPasswordAsync(AntigaPassEntry.Text, NovaPassEntry.Text);
        
        Indicador.IsRunning = false;

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
            AntigaPassEntry.Text = NovaPassEntry.Text = string.Empty;
        }
        else
        {
            await DisplayAlertAsync("Erro detalhado", resultado.Mensagem, "OK");
        }
    }    
}