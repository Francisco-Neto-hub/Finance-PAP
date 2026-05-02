using FinanceUI.Views;

namespace FinanceUI
{
    public partial class App : Application
    {
        // Ao colocarmos 'LoginPage' no construtor, o MAUI resolve as dependências sozinho
        [Obsolete]
        public App(LoginPage loginPage)
        {
            InitializeComponent();

            // Definimos que a página inicial é a LoginPage dentro de uma NavigationPage
            // A NavigationPage permite que faças "PushAsync" para outras páginas depois
            MainPage = new NavigationPage(loginPage);
        }
    }
}