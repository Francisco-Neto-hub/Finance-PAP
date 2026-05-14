using FinanceUI.Services;
using System.Diagnostics;

namespace FinanceUI.Views;

public partial class SuportePage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    public SuportePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnEnviarTicketClicked(object sender, EventArgs e)
    {
        if (_isBusy) return;

        // 1. Captura e Limpeza de Dados
        var assunto = txtAssunto?.Text?.Trim() ?? string.Empty;
        var mensagemCorpo = txtMensagem?.Text?.Trim() ?? string.Empty;

        // 2. Validação básica de UI
        if (string.IsNullOrWhiteSpace(assunto) || string.IsNullOrWhiteSpace(mensagemCorpo))
        {
            await DisplayAlertAsync("Atenção", "Por favor, preencha o assunto e a mensagem para podermos ajudar.", "OK");
            return;
        }

        try
        {
            // 3. Bloqueio de UI e Feedback
            _isBusy = true;
            btnEnviar.IsEnabled = false;
            indicator.IsRunning = true;

            // 4. Chamada ao ApiService (Tupla: sucesso e mensagem)
            var (sucesso, mensagemApi) = await _apiService.EnviarTicketAsync(assunto, mensagemCorpo);

            if (sucesso)
            {
                // Feedback de sucesso com a mensagem vinda do backend
                await DisplayAlertAsync("Sucesso ✨", mensagemApi ?? "Ticket enviado com sucesso! Responderemos em breve.", "OK");

                // Limpa os campos
                txtAssunto.Text = string.Empty;
                txtMensagem.Text = string.Empty;

                // 5. Navegação Segura de volta
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    // Fallback para Shell se estiveres a usar Shell Navigation
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                await DisplayAlertAsync("Erro no Envio", mensagemApi ?? "Não foi possível enviar o seu ticket agora.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Suporte Error]: {ex.Message}");
            await DisplayAlertAsync("Erro de Ligação", "Verifique a sua internet e tente novamente.", "OK");
        }
        finally
        {
            // 6. Restauro da UI
            _isBusy = false;
            indicator.IsRunning = false;
            btnEnviar.IsEnabled = true;
        }
    }
}