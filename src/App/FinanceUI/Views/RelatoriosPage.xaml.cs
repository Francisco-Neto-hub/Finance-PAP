using FinanceUI.Models;
using Microcharts;
using SkiaSharp; // Necessário para as cores
using System.Collections.ObjectModel;
using static FinanceUI.Models.RelatoriosDTO;

namespace FinanceUI.Views;

public partial class RelatoriosPage : ContentPage
{
    private readonly ApiService _apiService;

    public ObservableCollection<RelatorioTransacaoDTO> Movimentos { get; set; } = new();

    public RelatoriosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        ListaExtrato.ItemsSource = Movimentos;

        ConfigurarFiltros();
        CarregarContas();
        CarregarCategorias();
    }

    private async void CarregarContas()
    {
        try
        {
            var contas = await _apiService.GetContasAsync();

            if (contas != null)
            {
                var todas = new ContaDTO { Id = 0, NomeConta = "📊 Todas as Contas" };
                contas.Insert(0, todas);

                PickerConta.ItemsSource = contas;
                PickerConta.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar as contas.", "OK");
        }
    }

    private async void CarregarCategorias()
    {
        try
        {
            var categorias = await _apiService.GetCategoriasAsync();

            if (categorias != null)
            {
                var todas = new CategoriaDTO { IdCategoria = 0, Nome = "📂 Todas as Categorias" };
                categorias.Insert(0, todas);

                PickerCategoria.ItemsSource = categorias;
                PickerCategoria.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar as categorias.", "OK");
        }
    }

    private void ConfigurarFiltros()
    {
        PickerMes.ItemsSource = new List<string>
        {
            "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
            "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
        };

        var anos = new List<int>();
        for (int i = 2023; i <= DateTime.Now.Year + 1; i++) anos.Add(i);
        PickerAno.ItemsSource = anos;

        PickerMes.SelectedIndex = DateTime.Now.Month - 1;
        PickerAno.SelectedItem = DateTime.Now.Year;
    }

    private async void AoClicarProcurar(object sender, EventArgs e)
    {
        if (PickerMes.SelectedIndex == -1 || PickerAno.SelectedItem == null)
        {
            await DisplayAlertAsync("Aviso", "Selecione um mês e um ano.", "OK");
            return;
        }

        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;
            Movimentos.Clear();

            var contaSelecionada = (ContaDTO)PickerConta.SelectedItem;
            int? idConta = contaSelecionada?.Id > 0 ? contaSelecionada.Id : null;

            // NOVO: Capturar Categoria selecionada
            var catSelecionada = (CategoriaDTO)PickerCategoria.SelectedItem;
            int? idCategoria = catSelecionada?.IdCategoria > 0 ? catSelecionada.IdCategoria : null;

            // NOVO: Capturar texto da pesquisa
            string busca = SearchBarBusca.Text;

            int mes = PickerMes.SelectedIndex + 1;
            int ano = (int)PickerAno.SelectedItem;

            // Adicionamos , null, null no final para preencher os parâmetros que faltam
            var dados = await _apiService.GetExtratoDetalhadoAsync(mes, ano, idConta, idCategoria, busca);

            if (dados != null && dados.Count > 0)
            {
                foreach (var item in dados)
                {
                    Movimentos.Add(item);
                }
                LblResumoPesquisa.Text = $"Encontrados {dados.Count} movimentos:";
                LblResumoPesquisa.IsVisible = true;
            }
            else
            {
                LblResumoPesquisa.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Falha ao carregar relatório: " + ex.Message, "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }
}