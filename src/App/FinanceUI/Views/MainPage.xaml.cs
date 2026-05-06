using FinanceUI;
using FinanceUI.Models;
using FinanceUI.Views;

namespace FinanceUI
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly TransactionPage _transactionPage;

        // O MAUI injeta automaticamente tanto o serviço como a página de transação
        public MainPage(ApiService apiService, TransactionPage transactionPage)
        {
            InitializeComponent();
            _apiService = apiService;
            _transactionPage = transactionPage;
        }       

        // Método chamado ao clicar no botão "+"
        private async void AoClicarNovaTransacao(object sender, EventArgs e)
        {
            // Navega para a página de registo de transação
            await Navigation.PushAsync(_transactionPage);
        }

        private async void AoClicarSair(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlertAsync("Sair", "Deseja realmente encerrar a sessão?", "Sim", "Não");

            if (confirmar)
            {
                // 1. Remove o token
                SecureStorage.Default.Remove("auth_token");

                // 2. Redireciona para o Login
                // Resolvemos a LoginPage a partir do contentor de serviços para manter a DI
                var loginPage = Handler.MauiContext.Services.GetService<LoginPage>();
                Application.Current.MainPage = new NavigationPage(loginPage);
            }
        }

        // Método que puxa o resumo do Dashboard
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var resumo = await _apiService.GetResumoDashboardAsync();

            if (resumo != null)
            {
                // "C" formata para Moeda local (ex: R$ 3.001,00 ou 3.001,00 €)
                LblSaldoTotal.Text = resumo.SaldoTotal.ToString("C");

                // Atualiza as novas labels de resumo mensal
                LblReceitas.Text = resumo.TotalReceitasMes.ToString("C");
                LblDespesas.Text = resumo.TotalDespesasMes.ToString("C");

                // Se quiseres preencher a lista de transações:
                ListaTransacoes.ItemsSource = resumo.UltimasTransacoes;
            }
            else
            {
                LblSaldoTotal.Text = "Erro ao carregar";
            }

            // 2. Carregar a nova lista de Contas específica
            List<ContaDTO> contas = await _apiService.GetContasAsync();
            if (contas != null && contas.Count > 0)
            {
                ListaContas.ItemsSource = resumo.Contas;
            }
        }
    }
}