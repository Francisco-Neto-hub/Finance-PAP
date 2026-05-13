namespace FinanceUI.Views;

public partial class SuportePage : ContentPage
{
    private readonly ApiService _apiService;
    public SuportePage(ApiService apiService)
	{
		InitializeComponent();
        _apiService = apiService;
    }

    private async void OnEnviarTicketClicked(object sender, EventArgs e)
    {
        // Validação básica UI
        if (string.IsNullOrWhiteSpace(txtAssunto.Text) || string.IsNullOrWhiteSpace(txtMensagem.Text))
        {
            await DisplayAlertAsync("Atenção", "Preencha o assunto e a mensagem.", "OK");
            return;
        }

        btnEnviar.IsEnabled = false;
        indicator.IsRunning = true;

        // Chamada ao ApiService que configuramos acima
        var (sucesso, mensagem) = await _apiService.EnviarTicketAsync(txtAssunto.Text, txtMensagem.Text);

        indicator.IsRunning = false;
        btnEnviar.IsEnabled = true;

        if (sucesso)
        {
            // O "mensagem" aqui já é o texto vindo do seu backend: "Ticket enviado com sucesso!..."
            await DisplayAlertAsync("Sucesso", mensagem, "OK");

            // Limpa e volta
            txtAssunto.Text = string.Empty;
            txtMensagem.Text = string.Empty;
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlertAsync("Erro", mensagem, "OK");
        }
    }
}