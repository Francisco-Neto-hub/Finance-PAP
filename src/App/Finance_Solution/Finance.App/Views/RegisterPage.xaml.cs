using Finance.App.ViewModels;
namespace Finance.App.Views
{
    public partial class RegisterPage : ContentPage
    {
        // O MAUI injeta automaticamente o RegisterViewModel aqui 
        // desde que o tenhas registado no MauiProgram.cs
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();

            // Liga a UI ao ViewModel
            BindingContext = viewModel;
        }

        // Dica: Se quiseres limpar os campos sempre que a página aparece
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Se o teu ViewModel tiver um método de reset, podes chamá-lo aqui
            // Ex: ((RegisterViewModel)BindingContext).LimparCampos();
        }
    }
}