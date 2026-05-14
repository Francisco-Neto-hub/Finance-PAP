using FinanceUI.Models;
using FinanceUI.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static FinanceUI.Models.RelatoriosDTO;

namespace FinanceUI.Views;

public partial class RelatoriosPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    // ObservableCollection é excelente para a UI atualizar automaticamente ao adicionar itens
    public ObservableCollection<RelatorioTransacaoDTO> Movimentos { get; set; } = new();

    public RelatoriosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        // Vincula a coleção à lista no XAML
        ListaExtrato.ItemsSource = Movimentos;

        ConfigurarFiltrosIniciais();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carregamos os dados dos Pickers sempre que a página aparece para garantir que estão atualizados
        await Task.WhenAll(CarregarContas(), CarregarCategorias());
    }

    private async Task CarregarContas()
    {
        try
        {
            var contas = await _apiService.GetContasAsync();
            if (contas != null)
            {
                var todas = new ContaDTO { Id = 0, NomeConta = "📊 Todas as Contas" };
                contas.Insert(0, todas);

                PickerConta.ItemsSource = contas;
                if (PickerConta.SelectedIndex == -1) PickerConta.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Relatorios Error]: {ex.Message}");
        }
    }

    private async Task CarregarCategorias()
    {
        try
        {
            var categorias = await _apiService.GetCategoriasAsync();
            if (categorias != null)
            {
                var todas = new CategoriaDTO { IdCategoria = 0, Nome = "📂 Todas as Categorias" };
                categorias.Insert(0, todas);

                PickerCategoria.ItemsSource = categorias;
                if (PickerCategoria.SelectedIndex == -1) PickerCategoria.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Relatorios Error]: {ex.Message}");
        }
    }

    private void ConfigurarFiltrosIniciais()
    {
        PickerMes.ItemsSource = new List<string>
        {
            "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        };

        var anoAtual = DateTime.Now.Year;
        var anos = new List<int>();
        for (int i = anoAtual - 2; i <= anoAtual + 1; i++) anos.Add(i);

        PickerAno.ItemsSource = anos;
        PickerMes.SelectedIndex = DateTime.Now.Month - 1;
        PickerAno.SelectedItem = anoAtual;
    }

    private async void AoClicarProcurar(object sender, EventArgs e)
    {
        if (_isBusy) return;

        // 1. Validação de Filtros Básicos
        if (PickerMes.SelectedIndex == -1 || PickerAno.SelectedItem == null)
        {
            await DisplayAlertAsync("Aviso", "Selecione o mês e o ano para a pesquisa.", "OK");
            return;
        }

        try
        {
            _isBusy = true;
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            Movimentos.Clear();
            LblResumoPesquisa.IsVisible = false;

            // 2. Captura de filtros com Safe Cast
            var contaSelecionada = PickerConta.SelectedItem as ContaDTO;
            int? idConta = (contaSelecionada != null && contaSelecionada.Id > 0) ? contaSelecionada.Id : null;

            var catSelecionada = PickerCategoria.SelectedItem as CategoriaDTO;
            int? idCategoria = (catSelecionada != null && catSelecionada.IdCategoria > 0) ? catSelecionada.IdCategoria : null;

            string busca = SearchBarBusca.Text?.Trim();
            int mes = PickerMes.SelectedIndex + 1;
            int ano = (int)PickerAno.SelectedItem;

            // 3. Chamada à API
            var dados = await _apiService.GetExtratoDetalhadoAsync(mes, ano, idConta, idCategoria, busca);

            // 4. Alimentação da Lista
            if (dados != null && dados.Any())
            {
                foreach (var item in dados)
                {
                    Movimentos.Add(item);
                }
                LblResumoPesquisa.Text = $"Encontrados {dados.Count} movimentos";
                LblResumoPesquisa.IsVisible = true;
            }
            else
            {
                await DisplayAlertAsync("Informação", "Nenhum movimento encontrado para os filtros selecionados.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar o relatório detalhado.", "OK");
            Debug.WriteLine($"[Search Error]: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }
}