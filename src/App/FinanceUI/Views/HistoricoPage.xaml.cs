using FinanceUI.Models;
using FinanceUI.Services;
using System.Diagnostics;

namespace FinanceUI.Views;

public partial class HistoricoPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<ContaDTO> _contas;
    private bool _isBusy;

    public HistoricoPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Configuração inicial: últimos 30 dias
        DataInicioPicker.Date = DateTime.Now.AddDays(-30);
        DataFimPicker.Date = DateTime.Now;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Só recarrega as contas se a lista estiver vazia ou se quisermos atualizar saldos
        await CarregarContas();
    }

    private async Task CarregarContas()
    {
        try
        {
            _contas = await _apiService.GetContasAsync();

            if (_contas != null && _contas.Any())
            {
                ContaPicker.ItemsSource = _contas;

                // Seleciona a primeira conta automaticamente apenas se não houver seleção
                if (ContaPicker.SelectedIndex == -1)
                {
                    ContaPicker.SelectedIndex = 0;
                    // O trigger 'SelectedIndexChanged' (AoMudarConta) disparará o CarregarExtrato
                }
            }
            else
            {
                await DisplayAlertAsync("Aviso", "Não existem contas ativas para consultar o histórico.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Historico Error]: {ex.Message}");
        }
    }

    private async Task CarregarExtrato()
    {
        // 1. Verificações de segurança
        if (_isBusy) return;

        var contaSelecionada = (ContaDTO)ContaPicker.SelectedItem;
        if (contaSelecionada == null) return;

        // 2. Validação de intervalo de datas
        if (DataInicioPicker.Date > DataFimPicker.Date)
        {
            await DisplayAlertAsync("Intervalo Inválido", "A data de início não pode ser superior à data de fim.", "OK");
            return;
        }

        try
        {
            _isBusy = true;
            Indicador.IsRunning = true;
            ListaTransacoes.ItemsSource = null; // Limpa a lista para feedback visual

            // 3. Chamada à API
            var extrato = await _apiService.ObterExtratoAsync(
                contaSelecionada.Id,
                DataInicioPicker.Date,
                DataFimPicker.Date);

            // 4. Atualizar UI
            ListaTransacoes.ItemsSource = extrato ?? new List<TransacaoReadDTO>();

            if (extrato == null || !extrato.Any())
            {
                // Opcional: Podias mostrar um Label de "Sem transações neste período"
                Debug.WriteLine("Nenhuma transação encontrada para este período.");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar o extrato. Tente novamente.", "OK");
            Debug.WriteLine($"[Extrato Error]: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
            Indicador.IsRunning = false;
            RefreshExtrato.IsRefreshing = false;
        }
    }

    private async void AoMudarConta(object sender, EventArgs e)
    {
        // Eventos de UI devem ser 'async void' e chamar o Task com await
        await CarregarExtrato();
    }

    private async void AoClicarFiltrar(object sender, EventArgs e)
    {
        if (ContaPicker.SelectedIndex != -1)
        {
            await CarregarExtrato();
        }
        else
        {
            await DisplayAlertAsync("Aviso", "Selecione uma conta para filtrar.", "OK");
        }
    }

    private async void AoAtualizarLista(object sender, EventArgs e)
    {
        // Pull-to-refresh
        await CarregarExtrato();
    }
}