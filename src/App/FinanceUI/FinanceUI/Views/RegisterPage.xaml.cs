namespace FinanceUI;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void AoClicarRegistar(object sender, EventArgs e)
    {
        // 1. Validações básicas
        if (string.IsNullOrWhiteSpace(NomeEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PassEntry.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha os campos obrigatórios.", "OK");
            return;
        }

        if (PassEntry.Text != ConfirmPassEntry.Text)
        {
            await DisplayAlert("Erro", "As palavras-passe não coincidem!", "OK");
            return;
        }

        // 2. Iniciar animação de carregamento
        Indicador.IsRunning = true;
        BtnRegistar.IsEnabled = false;

        try
        {
            // AQUI: No futuro, chamaremos o teu ApiService.PostAsync("/api/Auth/registo", ...)
            await Task.Delay(2000); // Simulação de rede

            await DisplayAlert("Sucesso", "Conta criada com sucesso! Faça login agora.", "OK");

            // Volta para a página de Login
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha no registo: {ex.Message}", "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
            BtnRegistar.IsEnabled = true;
        }
    }
}