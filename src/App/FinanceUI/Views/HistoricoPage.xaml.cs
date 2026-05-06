using FinanceUI.Models;

namespace FinanceUI.Views;

public partial class HistoricoPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<ContaExemplo> _contas;

    public HistoricoPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Por defeito, os filtros mostram os últimos 30 dias
        DataInicioPicker.Date = DateTime.Now.AddDays(-30);
        DataFimPicker.Date = DateTime.Now;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContas();
    }

    private async Task CarregarContas()
    {
        _contas = await _apiService.GetContasAsync();

        if (_contas != null && _contas.Count > 0)
        {
            ContaPicker.ItemsSource = _contas;

            // Seleciona a primeira conta automaticamente se nada estiver selecionado
            if (ContaPicker.SelectedIndex == -1)
                ContaPicker.SelectedIndex = 0;
        }
    }

    private async Task CarregarExtrato()
    {
        var contaSelecionada = (ContaExemplo)ContaPicker.SelectedItem;
        if (contaSelecionada == null) return;

        ListaTransacoes.ItemsSource = null;
        Indicador.IsRunning = true;

        try
        {
            // Buscar dados à API com os filtros de data
            var extrato = await _apiService.ObterExtratoAsync(
                contaSelecionada.Id,
                DataInicioPicker.Date,
                DataFimPicker.Date);

            ListaTransacoes.ItemsSource = extrato;
        }
        catch (Exception ex)
        {
            // Se a API estiver a barrar o pedido (ex: erro no ID do Cliente), vai aparecer aqui!
            await DisplayAlertAsync("Erro a carregar", ex.Message, "OK");
        }
        finally
        {
            Indicador.IsRunning = false;
        }
    }

    private void AoMudarConta(object sender, EventArgs e)
    {
        if (ContaPicker.SelectedIndex != -1)
        {
            CarregarExtrato();
        }
    }

    private void AoClicarFiltrar(object sender, EventArgs e)
    {
        if (ContaPicker.SelectedIndex != -1)
        {
            CarregarExtrato();
        }
        else
        {
            DisplayAlertAsync("Aviso", "Por favor, selecione uma conta primeiro.", "OK");
        }
    }

    private async void AoAtualizarLista(object sender, EventArgs e)
    {
        await CarregarExtrato();
        RefreshExtrato.IsRefreshing = false; // Pára a animação do pull-to-refresh
    }
}