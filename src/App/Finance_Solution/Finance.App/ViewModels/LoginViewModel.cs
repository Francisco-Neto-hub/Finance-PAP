using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Core.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Finance.App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            // Instanciar o comando manualmente
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        // Propriedades Manuais (Backing Fields)
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isBusy;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Propriedade do Comando
        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                // Valida na BD (SQLite) através do teu AuthService
                var usuario = await _authService.ValidarCredenciaisAsync(Email, Password);

                if (usuario != null)
                {
                    // SUCESSO: Entra na Main Page
                    // O "//" limpa o histórico para o utilizador não voltar ao login com o botão "back"
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    // FALHA: Dados inválidos
                    await Shell.Current.DisplayAlertAsync("Acesso Negado", "Email ou Password incorretos.", "Tentar novamente");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGIN_ERROR]: {ex.Message}");
                // Isto vai mostrar o erro real no ecrã para sabermos se é "Table not found" ou "Wrong password"
                await Shell.Current.DisplayAlertAsync("Erro de Ligação", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task IrParaRegisto()
        {
            // Navegação simples para a página de registo
            await Shell.Current.GoToAsync("RegisterPage");
        }
    }
}
