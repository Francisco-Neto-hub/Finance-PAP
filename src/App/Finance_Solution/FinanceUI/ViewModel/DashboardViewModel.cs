using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Core.Models;
using Finance.Core.Services;
using System.Collections.ObjectModel;

namespace FinanceUI.ViewModel;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IFinanceService _financeService;
    private readonly int _idContratoAtivo; // Obtido após o login

    [ObservableProperty]
    public partial decimal SaldoTotal { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<Transacao> UltimasTransacoes { get; } = new();

    public DashboardViewModel(IFinanceService financeService, int idContrato)
    {
        _financeService = financeService;
        _idContratoAtivo = idContrato;
    }

    [RelayCommand]
    public async Task CarregarDadosAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // 1. Procura o Saldo Total
            SaldoTotal = await _financeService.GetSaldoTotalContratoAsync(_idContratoAtivo);

            // 2. Procura as últimas 10 transações
            var transacoes = await _financeService.GetUltimasTransacoesAsync(_idContratoAtivo);

            UltimasTransacoes.Clear();
            foreach (var t in transacoes)
                UltimasTransacoes.Add(t);
        }
        finally
        {
            IsBusy = false;
        }
    }
}