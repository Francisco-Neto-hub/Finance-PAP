using FinanceUI.Models;

namespace FinanceUI.Views;

public partial class ContasPage : ContentPage
{
    private readonly ApiService _apiService;
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
        RefreshContas.IsRefreshing = true;
        List<ContaDTO> contas = await _apiService.GetContasAsync();
        ListaContas.ItemsSource = contas;
        RefreshContas.IsRefreshing = false;
    }

    private void AoAtualizarLista(object sender, EventArgs e)
    {
        _ = CarregarContas();
    }

    // --- LÓGICA DE CRIAR CONTA ---
    private void AoClicarMostrarFormulario(object sender, EventArgs e)
    {
        FormNovaConta.IsVisible = true;
    }

    private void AoClicarCancelar(object sender, EventArgs e)
    {
        FormNovaConta.IsVisible = false;
        NomeContaEntry.Text = string.Empty;
        SaldoInicialEntry.Text = string.Empty;
    }

    private async void AoClicarGuardarConta(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NomeContaEntry.Text))
        {
            await DisplayAlertAsync("Erro", "Dê um nome à conta.", "OK");
            return;
        }

        // Converte o saldo introduzido (trata vírgulas e pontos)
        decimal saldoInicial = 0;
        if (!string.IsNullOrWhiteSpace(SaldoInicialEntry.Text))
        {
            if (!decimal.TryParse(SaldoInicialEntry.Text.Replace('.', ','), out saldoInicial))
            {
                await DisplayAlertAsync("Erro", "O saldo inserido é inválido.", "OK");
                return;
            }
        }

        var resultado = await _apiService.CriarContaAsync(NomeContaEntry.Text, saldoInicial);

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
            AoClicarCancelar(null, null); // Esconde e limpa o formulário
            await CarregarContas();       // Atualiza a lista
        }
        else
        {
            await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
        }
    }

    // --- LÓGICA DE FECHAR CONTA (SWIPE) ---
    private async void AoFazerSwipeFechar(object sender, EventArgs e)
    {
        var item = (SwipeItem)sender;
        var contaSelecionada = (ContaDTO)item.CommandParameter;

        if (contaSelecionada == null) return;

        bool confirmar = await DisplayAlertAsync("Confirmação", $"Tem a certeza que deseja fechar a conta '{contaSelecionada.NomeConta}'?", "Sim", "Não");
        if (!confirmar) return;

        var resultado = await _apiService.FecharContaAsync(contaSelecionada.Id);

        if (resultado.Sucesso)
        {
            await DisplayAlertAsync("Fechada", resultado.Mensagem, "OK");
            await CarregarContas(); // Remove da lista visualmente
        }
        else
        {
            await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
        }
    }
}