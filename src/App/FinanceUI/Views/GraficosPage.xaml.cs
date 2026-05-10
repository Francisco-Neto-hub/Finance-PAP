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
        CarregarContas();
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
        catch (Exception)
        {
            await DisplayAlertAsync("Erro", "Não foi possível carregar as contas.", "OK");
        }
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

            var contaSelecionada = (ContaDTO)PickerConta.SelectedItem;
            int? idConta = contaSelecionada?.Id > 0 ? contaSelecionada.Id : null;

            int mes = PickerMes.SelectedIndex + 1;
            int ano = (int)PickerAno.SelectedItem;

            // Busca os dados aos novos endpoints separados
            // Atualiza a chamada para passar o idConta
            var dadosDespesas = await _apiService.GetGastosPorCategoriaAsync(mes, ano, idConta);
            var dadosFluxo = await _apiService.GetFluxoCaixaAsync(ano, idConta);
          
            // Processa e desenha os gráficos
            ConfigurarGraficoDonut(dadosDespesas);
            DesenharGraficoFluxo(dadosFluxo);
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
    private void DesenharGraficoFluxo(List<FluxoCaixaDTO> dados)
    {
        if (dados == null || dados.Count == 0) return;

        var entries = new List<ChartEntry>();

        foreach (var item in dados)
        {
            // Barra de Receitas
            entries.Add(new ChartEntry((float)item.TotalReceitas)
            {
                Label = GetNomeMes(item.Mes),
                ValueLabel = item.TotalReceitas.ToString("C"),
                Color = SKColor.Parse("#2ecc71")
            });

            // Barra de Despesas
            entries.Add(new ChartEntry((float)item.TotalDespesas)
            {
                ValueLabel = item.TotalDespesas.ToString("C"),
                Color = SKColor.Parse("#e74c3c")
            });
        }

        // O SEGREDO ESTÁ AQUI: Forçar o MAUI a desenhar no ecrã principal!
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ChartBarras.Chart = new BarChart
            {
                Entries = entries,

                // 1. Reduzir o tamanho da letra (12 a 14 é o ideal para telemóveis)
                LabelTextSize = 18,

                // 2. Forçar as etiquetas a ficarem na horizontal
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,

                BackgroundColor = SKColors.Transparent,
                Margin = 20
            };
        });
    }

    // Função auxiliar para converter número do mês em nome
    private string GetNomeMes(int mes)
    {
        string[] meses = { "", "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        return meses[mes];
    }
}