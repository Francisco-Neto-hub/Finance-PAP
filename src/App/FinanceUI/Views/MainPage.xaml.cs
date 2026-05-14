using FinanceUI.Models;
using FinanceUI.Services;
using FinanceUI.Views;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;

namespace FinanceUI;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly TransactionPage _transactionPage;
    private bool _isRefreshing = false;

    public MainPage(ApiService apiService, TransactionPage transactionPage)
    {
        InitializeComponent();
        _apiService = apiService;
        _transactionPage = transactionPage;
    }

    // Executado sempre que a página aparece (útil para atualizar após uma nova transação)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosDashboard();
    }

    private async Task CarregarDadosDashboard()
    {
        if (_isRefreshing) return;

        try
        {
            _isRefreshing = true;

            var resumo = await _apiService.GetResumoDashboardAsync();

            if (resumo != null)
            {
                // 1. Atualizar Labels de Valores (Formatação de Moeda)
                LblSaldoTotal.Text = resumo.SaldoTotal.ToString("C");
                LblReceitas.Text = resumo.TotalReceitasMes.ToString("C");
                LblDespesas.Text = resumo.TotalDespesasMes.ToString("C");

                // 2. Atualizar Listas (Sempre verificar se há dados)
                ListaTransacoes.ItemsSource = resumo.UltimasTransacoes;

                // 3. Atualizar Gráfico apenas se houver dados de categorias
                ConfigurarGraficoDashboard(resumo.GastosPorCategoria);

                // 4. Carregar Contas (se a lista for diferente da do resumo)
                var contas = await _apiService.GetContasAsync();
                if (contas != null)
                {
                    ListaContas.ItemsSource = contas;
                }
            }
            else
            {
                await DisplayAlertAsync("Erro", "Não foi possível sincronizar os dados.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Dashboard Error]: {ex.Message}");
            // Evita crashar se a API falhar momentaneamente
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void ConfigurarGraficoDashboard(List<GastoCategoriaResumoDTO> dados)
    {
        if (dados == null || !dados.Any())
        {
            ChartDashboard.Chart = null; // Ou um gráfico "vazio" elegante
            return;
        }

        // Paleta "Premium" (Azuis, Roxos e um toque de alerta)
        string[] paletaCores = { "#2563EB", "#7C3AED", "#DB2777", "#F59E0B", "#10B981", "#6366F1" };

        var entradas = dados.Select((d, index) => new ChartEntry((float)d.TotalGasto)
        {
            Label = d.Categoria,
            ValueLabel = d.TotalGasto.ToString("N2") + " €",
            Color = SKColor.Parse(paletaCores[index % paletaCores.Length]),
            TextColor = SKColors.White, // Garante visibilidade no tema escuro
            ValueLabelColor = SKColor.Parse(paletaCores[index % paletaCores.Length])
        }).ToArray();

        ChartDashboard.Chart = new DonutChart
        {
            Entries = entradas,
            LabelTextSize = 26,
            HoleRadius = 0.65f, // Buraco central maior fica mais moderno
            BackgroundColor = SKColors.Transparent,
            GraphPosition = GraphPosition.Center,
            Margin = 20
        };
    }

    private async void AoClicarNovaTransacao(object sender, EventArgs e)
    {
        if (_transactionPage == null) return;
        await Navigation.PushAsync(_transactionPage);
    }

    private async void AoClicarSair(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync("Sair", "Deseja realmente encerrar a sessão?", "Sim", "Não");

        if (confirmar)
        {
            try
            {
                // 1. Limpa o token de segurança
                SecureStorage.Default.Remove("auth_token");

                // 2. Resolvemos a LoginPage via DI para garantir que ela recebe o ApiService
                var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();

                // 3. Reinicia a App na página de Login (limpa a stack de navegação e o Shell)
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logout Error]: {ex.Message}");
                // Fallback caso a DI falhe por algum motivo
                Application.Current.MainPage = new NavigationPage(new LoginPage(_apiService));
            }
        }
    }
}