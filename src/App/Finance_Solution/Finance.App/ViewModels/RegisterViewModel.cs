using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Core.Models;
using Finance.Core.Services;
using System.Diagnostics;

namespace Finance.App.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        // --- CAMPOS PRIVADOS ---
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isBusy;

        // --- PROPRIEDADES MANUAIS (Para satisfazer o WinUI 3/AOT) ---
        public string Nome
        {
            get => _nome;
            set => SetProperty(ref _nome, value);
        }

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

        // --------------------------------------------------------

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            // IMPORTANTE: Aqui usas as propriedades que o Toolkit GEROU (em Maiúsculas)
            // O Toolkit pega em "_nome" e cria "Nome" automaticamente.
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Preencha todos os campos.", "OK");
                return;
            }

            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var novoCliente = new Cliente
                {
                    Nome = Nome,
                    Email = Email,
                    ByPass = Password,
                    IdEstadoCliente = 1,
                    DataCriacao = DateTime.Now
                };

                var sucesso = await _authService.RegistarClienteAsync(novoCliente);

                if (sucesso)
                {
                    await Shell.Current.DisplayAlertAsync("Sucesso", "Conta criada!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Erro", "Email já registado.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO]: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task VoltarLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
