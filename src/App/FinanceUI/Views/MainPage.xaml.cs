using FinanceUI;
using FinanceUI.Models;

namespace FinanceUI
{
    public partial class MainPage : ContentPage
    {
        // Instanciamos o serviço que vai comunicar com a API
        private readonly ApiService _apiService = new ApiService();

        public MainPage()
        {
            InitializeComponent();
        }

        // Este método é executado automaticamente sempre que a página aparece no ecrã
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarDadosReais();
        }

        private async Task CarregarDadosReais()
        {
            try
            {
                // NOTA: Por agora, como a tua API usa [Authorize], 
                // precisas de um token que viria do Login.
                string tokenTemporario = "COLA_AQUI_UM_TOKEN_DO_SWAGGER";

                // Chamada ao endpoint GetMinhasContas da tua API
                var contas = await _apiService.GetContasAsync();

                if (contas != null && contas.Any())
                {
                    // cvContas é o nome da CollectionView que deve estar no teu MainPage.xaml
                    cvContas.ItemsSource = contas;

                    // lblSaldo é a Label que mostra o somatório dos saldos
                    lblSaldo.Text = contas.Sum(c => c.Saldo).ToString("C");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Erro", "Não foi possível carregar os dados: " + ex.Message, "OK");
            }
        }
    }
}