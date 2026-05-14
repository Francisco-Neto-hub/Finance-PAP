using FinanceUI.Models;
using FinanceUI.Services;
using System.Diagnostics;
using System.Globalization;

namespace FinanceUI.Views;

public partial class ContasPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    public ContasPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContas();
    }

    private async Task CarregarContas()
    {
        if (_isBusy) return;

        try
        {
            _isBusy = true;
            RefreshContas.IsRefreshing = true;

            var contas = await _apiService.GetContasAsync();

            // Atribui a lista (se for null, fica vazia para evitar erro na UI)
            ListaContas.ItemsSource = contas ?? new List<ContaDTO>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Contas Error]: {ex.Message}");
            await DisplayAlertAsync("Erro", "Não foi possível carregar as contas.", "OK");
        }
        finally
        {
            RefreshContas.IsRefreshing = false;
            _isBusy = false;
        }
    }

    private async void AoAtualizarLista(object sender, EventArgs e)
    {
        await CarregarContas();
    }

    // --- LÓGICA DE CRIAR CONTA ---

    private void AoClicarMostrarFormulario(object sender, EventArgs e)
    {
        FormNovaConta.IsVisible = true;
        NomeContaEntry.Focus(); // Dá foco automático ao teclado
    }

    private void AoClicarCancelar(object sender, EventArgs e)
    {
        FormNovaConta.IsVisible = false;
        NomeContaEntry.Text = string.Empty;
        SaldoInicialEntry.Text = string.Empty;
    }

    private async void AoClicarGuardarConta(object sender, EventArgs e)
    {
        // 1. Validação de Nome
        var nomeConta = NomeContaEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(nomeConta))
        {
            await DisplayAlertAsync("Aviso", "Por favor, dê um nome à conta (ex: Carteira, Banco).", "OK");
            return;
        }

        // 2. Validação de Saldo (Otimizado para ser universal)
        string saldoTexto = SaldoInicialEntry.Text?.Replace(',', '.') ?? "0";
        if (!decimal.TryParse(saldoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal saldoInicial))
        {
            await DisplayAlertAsync("Erro", "O saldo inicial inserido é inválido.", "OK");
            return;
        }

        try
        {
            _isBusy = true;
            // Opcional: Desativar botão de guardar enquanto processa

            var resultado = await _apiService.CriarContaAsync(nomeConta, saldoInicial);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso ✨", resultado.Mensagem, "OK");
                AoClicarCancelar(null, null); // Limpa e esconde o form
                await CarregarContas();       // Atualiza a lista
            }
            else
            {
                await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlertAsync("Erro Crítico", "Falha ao comunicar com o servidor.", "OK");
        }
        finally
        {
            _isBusy = false;
        }
    }

    // --- LÓGICA DE FECHAR CONTA (SWIPE) ---

    private async void AoFazerSwipeFechar(object sender, EventArgs e)
    {
        // 1. Obter o item selecionado via CommandParameter
        if (sender is not SwipeItem swipeItem || swipeItem.CommandParameter is not ContaDTO contaSelecionada)
            return;

        // 2. Confirmação de segurança
        bool confirmar = await DisplayAlertAsync("Fechar Conta",
            $"Deseja mesmo fechar a conta '{contaSelecionada.NomeConta}'?\n\nEsta ação não pode ser revertida.",
            "Confirmar", "Cancelar");

        if (!confirmar) return;

        try
        {
            var resultado = await _apiService.FecharContaAsync(contaSelecionada.Id);

            if (resultado.Sucesso)
            {
                await DisplayAlertAsync("Sucesso", "Conta encerrada com sucesso.", "OK");
                await CarregarContas();
            }
            else
            {
                await DisplayAlertAsync("Não foi possível fechar", resultado.Mensagem, "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Swipe Error]: {ex.Message}");
            await DisplayAlertAsync("Erro", "Ocorreu um erro ao tentar fechar a conta.", "OK");
        }
    }
}