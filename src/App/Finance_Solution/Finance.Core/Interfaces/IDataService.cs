using Finance.Core.Models;

namespace Finance.Core.Interfaces
{
    public interface IDataService
    {
        // Login
        Task<int?> ValidarLoginAsync(string email, string password);

        // Transações
        Task<bool> AdicionarMovimentoAsync(int contaId, string titulo, decimal valor, int categoriaId, int tipoId);

        // Consultas
        Task<IEnumerable<Conta>> GetResumoContasAsync();
    }
}
