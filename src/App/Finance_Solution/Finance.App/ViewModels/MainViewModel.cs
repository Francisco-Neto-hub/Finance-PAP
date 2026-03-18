using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Core.Models;
using Finance.Core.Services;
using Microcharts;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Finance.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IFinanceService _financeService;
        private readonly int _usuarioLogadoId = 1;

        // --- PROPRIEDADES MANUAIS (Para evitar erros de AOT e Ambiguidade no Windows) ---
        private decimal _saldoTotal;
        public decimal SaldoTotal
        {
            get => _saldoTotal;
            set => SetProperty(ref _saldoTotal, value);
        }

        private Chart? _spendingChart;
        public Chart? SpendingChart
        {
            get => _spendingChart;
            set => SetProperty(ref _spendingChart, value);
        }

        public ObservableCollection<Transacao> Transacoes { get; set; } = new();

        public MainViewModel(IFinanceService financeService)
        {
            _financeService = financeService;

            // Inicializamos o gráfico vazio para evitar avisos de nulidade
            SpendingChart = new DonutChart { Entries = new List<ChartEntry>() };

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var valor = await _financeService.GetSaldoTotalContratoAsync(_usuarioLogadoId);
                var lista = await _financeService.GetUltimasTransacoesAsync(_usuarioLogadoId);

                SaldoTotal = valor;

                await MainThread.InvokeOnMainThreadAsync(() => {
                    Transacoes.Clear();
                    foreach (var t in lista) Transacoes.Add(t);

                    // 1. ATUALIZA O GRÁFICO assim que os dados chegam
                    UpdateChart(lista);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainVM Error]: {ex.Message}");
            }
        }

        public void UpdateChart(IEnumerable<Transacao> transacoes)
        {
            if (transacoes == null || !transacoes.Any()) return;

            try
            {
                // 1. Agrupar gastos por Categoria (usando o campo 'Nome' que confirmaste)
                var entries = transacoes
                    .GroupBy(t => t.IdCategoriaNavigation?.Nome ?? "Geral")
                    .Select(g => new ChartEntry(Math.Abs((float)g.Sum(t => t.ValorTransacao)))
                  {
                    Label = g.Key,
                    ValueLabel = g.Sum(t => t.ValorTransacao).ToString("N2") + "€",
                    Color = SKColor.Parse(GetRandomColor())
                  }).ToList();

                // 2. Atualizar a propriedade do gráfico
                SpendingChart = new DonutChart
                {
                    Entries = entries,
                    LabelTextSize = 24,
                    BackgroundColor = SKColors.Transparent,
                    LabelColor = SKColors.White,
                    Margin = 20
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Chart Error]: {ex.Message}");
            }
        }

        private string GetRandomColor()
        {
            string[] colors = { "#FF1744", "#00E676", "#2979FF", "#FFEA00", "#D500F9", "#FF9100" };
            return colors[new Random().Next(colors.Length)];
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Se quiseres confirmar antes de sair
            bool resposta = await Shell.Current.DisplayAlertAsync("Sair", "Desejas terminar a sessão?", "Sim", "Não");

            if (resposta)
            {
                // Navega para a raiz (Login) e limpa o histórico de navegação
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
