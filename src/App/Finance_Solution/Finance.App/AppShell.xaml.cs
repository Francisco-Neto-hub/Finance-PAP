using Finance.App.Views;

namespace Finance.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Isto permite fazer: Shell.Current.GoToAsync("RegisterPage");
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }
    }
}
