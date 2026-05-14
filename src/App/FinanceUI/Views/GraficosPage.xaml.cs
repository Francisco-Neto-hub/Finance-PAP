using FinanceUI.Models;
using FinanceUI.Services;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;
using static FinanceUI.Models.GraficosDTO;

namespace FinanceUI.Views;

public partial class GraficosPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    // Paleta de cores refinada para um look mais moderno
    private readonly string[] _paletaCores = {
        "#512BD4", "#AC94F4", "#3498db", "#e67e22",
        "#1abc9c", "#9b59b6", "#f1c40f", "#e74c3c"
    };

    public GraficosPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;

        ConfigurarFiltrosIniciais();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContas();
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
        for (int i = 2023; i <= anoAtual + 1; i++) anos.Add(i);

        PickerAno.ItemsSource = anos;
        PickerMes.SelectedIndex = DateTime.Now.Month - 1;
        PickerAno.SelectedItem = anoAtual;
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
            Debug.WriteLine($"[Graficos Error]: {ex.Message}");
        }
    }

    private async void AoClicarGerar(object sender, EventArgs e)
    {
        if (_isBusy) return;

        if (PickerMes.SelectedIndex == -1 || PickerAno.SelectedItem == null)
        {
            await DisplayAlertAsync("Aviso", "Selecione o mês e o ano para gerar o relatório.", "OK");
            return;
        }

        try
        {
            _isBusy = true;
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var contaSelecion = PickerConta.SelectedItem as ContaDTO;
            int? idConta = (contaSelecion != null && contaSelecion.Id > 0) ? contaSelecion.Id : null;
            int mes = PickerMes.SelectedIndex + 1;
            int ano = (int)PickerAno.SelectedItem;

            // PERFORMANCE: Disparamos as duas consultas à API ao mesmo tempo
            var tarefaDespesas = _apiService.GetGastosPorCategoriaAsync(mes, ano, idConta);
            var tarefaFluxo = _apiService.GetFluxoCaixaAsync(ano, idConta);

            await Task.WhenAll(tarefaDespesas, tarefaFluxo);

            // Processamos os resultados
            ConfigurarGraficoDonut(await tarefaDespesas);
            DesenharGraficoFluxo(await tarefaFluxo);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Chart Error]: {ex.Message}");
            await DisplayAlertAsync("Erro", "Falha ao processar dados dos gráficos.", "OK");
        }
        finally
        {
            _isBusy = false;
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private void ConfigurarGraficoDonut(List<GastoCategoriaDTO> dados)
    {
        if (dados == null || !dados.Any())
        {
            ChartDonut.Chart = null;
            return;
        }

        var entradas = dados.Select((d, index) => new ChartEntry((float)d.TotalGasto)
        {
            Label = d.Categoria,
            ValueLabel = d.TotalGasto.ToString("N0") + " €",
            Color = SKColor.Parse(_paletaCores[index % _paletaCores.Length]),
            TextColor = SKColors.White,
            ValueLabelColor = SKColor.Parse(_paletaCores[index % _paletaCores.Length])
        }).ToArray();

        ChartDonut.Chart = new DonutChart
        {
            Entries = entradas,
            LabelTextSize = 24,
            HoleRadius = 0.6f, // Estilo mais "thin" e elegante
            BackgroundColor = SKColors.Transparent,
            Margin = 15
        };
    }

    private void DesenharGraficoFluxo(List<FluxoCaixaDTO> dados)
    {
        if (dados == null || !dados.Any())
        {
            ChartBarras.Chart = null;
            return;
        }

        var entries = new List<ChartEntry>();

        foreach (var item in dados)
        {
            // Barra de Receitas (Verde)
            entries.Add(new ChartEntry((float)item.TotalReceitas)
            {
                Label = GetNomeMes(item.Mes),
                ValueLabel = item.TotalReceitas > 0 ? item.TotalReceitas.ToString("N0") : "",
                Color = SKColor.Parse("#2ecc71"),
                TextColor = SKColors.White
            });

            // Barra de Despesas (Vermelho)
            entries.Add(new ChartEntry((float)item.TotalDespesas)
            {
                ValueLabel = item.TotalDespesas > 0 ? item.TotalDespesas.ToString("N0") : "",
                Color = SKColor.Parse("#e74c3c"),
                TextColor = SKColors.White
            });
        }

        // Garante que a atualização do gráfico ocorre na UI Thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ChartBarras.Chart = new BarChart
            {
                Entries = entries,
                LabelTextSize = 18,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                BackgroundColor = SKColors.Transparent,
                Margin = 20,
                // Adiciona um espaçamento entre os grupos de meses
                BarAreaAlpha = 50
            };
        });
    }

    private string GetNomeMes(int mes)
    {
        string[] meses = { "", "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        return (mes >= 1 && mes <= 12) ? meses[mes] : "";
    }
}