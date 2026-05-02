namespace FinanceUI.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void AoClicarRegistar(object sender, EventArgs e)
    {
        // 1. Valida��es b�sicas
        if (string.IsNullOrWhiteSpace(NomeEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PassEntry.Text))
        {
            await DisplayAlertAsync("Erro", "Por favor, preencha os campos obrigat�rios.", "OK");
            return;
        }

        if (PassEntry.Text != ConfirmPassEntry.Text)
        {
            await DisplayAlertAsync("Erro", "As palavras-passe n�o coincidem!", "OK");
            return;
        }

        // 2. Iniciar anima��o de carregamento
        Indicador.IsRunning = true;
        BtnRegistar.IsEnabled = false;

        try
        {
            // AQUI: No futuro, chamaremos o teu ApiService.PostAsync("/api/Auth/registo", ...)
            await Task.Delay(2000); // Simula��o de rede

            await DisplayAlertAsync("Sucesso", "Conta criada com sucesso! Fa�a login agora.", "OK");

            // Volta para a p�gina de Login
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Falha no registo: {ex.Message}", "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
            BtnRegistar.IsEnabled = true;
        }
    }
}