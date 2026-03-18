using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Core.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace Finance.App.ViewModels
{
    public class LoginViewModel : ObservableObject
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

                var usuario = await _authService.ValidarCredenciaisAsync(Email, Password);

                if (usuario != null)
                {
                    await Shell.Current.DisplayAlertAsync("Sucesso", $"Bem-vindo, {usuario.Nome}", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Erro", "Email ou Password incorretos", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGIN_ERROR]: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Erro", "Falha na ligação à base de dados.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
