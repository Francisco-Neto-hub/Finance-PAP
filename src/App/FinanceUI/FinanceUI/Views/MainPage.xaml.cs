namespace FinanceUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            CarregarDadosTeste();
        }

        private void CarregarDadosTeste()
        {
            // Criamos uma lista fictícia para testar o design
            var lista = new List<dynamic>
            {
                new { NomeConta = "Conta Corrente", SaldoAtual = 1250.00m, Titular = "Francisco" },
                new { NomeConta = "Poupança", SaldoAtual = 500.25m, Titular = "Francisco" }
            };

            cvContas.ItemsSource = lista;
            lblSaldo.Text = "€ 1.750,25";
        }
    }
}