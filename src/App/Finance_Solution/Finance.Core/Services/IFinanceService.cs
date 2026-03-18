using Finance.Core.Models;

namespace Finance.Core.Services
{
    public interface IFinanceService
    {
        // Adicionamos o parâmetro idContrato para bater com o teu Service
        Task<decimal> GetSaldoTotalContratoAsync(int idContrato);

        // Adicionamos para a lista de transações que a Dashboard pede
        Task<List<Transacao>> GetUltimasTransacoesAsync(int idContrato);

        // Adicionamos o registo que já criaste no Service
        Task<(bool Sucesso, string Mensagem)> RegistarTransacaoAsync(Transacao movimento);

        // Novo: Para os gráficos que estão na tua checklist
        Task<Dictionary<string, decimal>> GetGastosPorCategoriaAsync(int idContrato);
    }
}
