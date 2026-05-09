using FinanceUI.Models;
using Microcharts;
using SkiaSharp;
using static FinanceUI.Models.GraficosDTO;

namespace FinanceUI.Views;

public partial class GraficosPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly string[] _paletaCores = { "#512BD4", "#AC94F4", "#C62828", "#FF9800", "#00BCD4", "#4CAF50", "#E91E63", "#9C27B0" };

    public GraficosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        ConfigurarFiltros();
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

    private async void AoClicarGerar(object sender, EventArgs e)
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

            int mes = PickerMes.SelectedIndex + 1;
            int ano = (int)PickerAno.SelectedItem;

            // Busca os dados aos novos endpoints separados
            var dadosCategoria = await _apiService.GetGastosPorCategoriaAsync(mes, ano);
            var dadosFluxo = await _apiService.GetFluxoCaixaAsync(ano);

            // Processa e desenha os gráficos
            ConfigurarGraficoDonut(dadosCategoria);
            ConfigurarGraficoBarras(dadosFluxo);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar os gráficos. " + ex.Message, "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private void ConfigurarGraficoDonut(List<GastoCategoriaDTO> dados)
    {
        if (dados == null || !dados.Any())
        {
            ChartDonut.Chart = new DonutChart { Entries = new ChartEntry[0], BackgroundColor = SKColors.Transparent };
            return;
        }

        var entradas = dados.Select((d, index) => new ChartEntry((float)d.TotalGasto)
        {
            Label = d.Categoria,
            ValueLabel = d.TotalGasto.ToString("N0") + " €", // N0 remove os cêntimos para gráficos mais limpos
            Color = SKColor.Parse(_paletaCores[index % _paletaCores.Length])
        }).ToArray();

        ChartDonut.Chart = new DonutChart
        {
            Entries = entradas,
            LabelTextSize = 24,
            HoleRadius = 0.5f,
            BackgroundColor = SKColors.Transparent
        };
    }

    private void ConfigurarGraficoBarras(List<FluxoCaixaDTO> dados)
    {
        if (dados == null || !dados.Any())
        {
            ChartBarras.Chart = new BarChart { Entries = new ChartEntry[0], BackgroundColor = SKColors.Transparent };
            return;
        }

        // Vamos mapear as DESPESAS ao longo do ano
        var entradas = dados.Select(d => new ChartEntry((float)d.TotalDespesas)
        {
            Label = d.NomeMes,
            ValueLabel = d.TotalDespesas > 0 ? d.TotalDespesas.ToString("N0") : "", // Esconde o label se for 0
            Color = SKColor.Parse("#C62828") // Vermelho para despesas
        }).ToArray();

        ChartBarras.Chart = new BarChart
        {
            Entries = entradas,
            LabelTextSize = 20,
            BackgroundColor = SKColors.Transparent,
            ValueLabelOrientation = Orientation.Horizontal
        };
    }
}