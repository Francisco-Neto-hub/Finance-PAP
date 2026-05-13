using FinanceUI.Views;

namespace FinanceUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registre as páginas que não estão fixas no menu lateral
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RecuperarPasswordPage), typeof(RecuperarPasswordPage));
        }
    }
}
