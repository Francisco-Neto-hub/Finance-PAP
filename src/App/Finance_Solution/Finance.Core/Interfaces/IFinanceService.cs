using Finance.Core.Models;
using Finance.Core.DTOs;

namespace Finance.Core.Interfaces;

public interface IFinanceService
{
    // Autenticação
    Task<int?> LoginAsync(string email, string password);

    // Gestão de Contas
    Task<IEnumerable<ResumoContaDTO>> ObterDashboardAsync();
    Task<bool> CriarContaAsync(Conta novaConta);

    // Movimentações
    Task<bool> RegistarTransacaoAsync(int contaId, string descricao, decimal valor, int categoriaId, int tipoId);
    Task<bool> RealizarTransferenciaAsync(int origemId, int destinoId, decimal valor);

    // Auxiliares
    Task<IEnumerable<Categoria>> ObterCategoriasAsync();
}