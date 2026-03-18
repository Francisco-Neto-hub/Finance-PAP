using Finance.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; // Necessário para o Debug.WriteLine

namespace Finance.Core.Services
{
    // 1. Assinar a interface explicitamente
    public class FinanceService : IFinanceService
    {
        private readonly FinanceDbContext _context;

        public FinanceService(FinanceDbContext context)
        {
            _context = context;
        }

        // --- MOVIMENTAÇÕES ---

        public async Task<(bool Sucesso, string Mensagem)> RegistarTransacaoAsync(Transacao movimento)
        {
            // 1. Validação de Entrada: Impede valores negativos ou zero na transação
            if (movimento.ValorTransacao <= 0)
                return (false, "O valor da transação deve ser superior a zero.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var conta = await _context.Conta.FindAsync(movimento.IdConta);
                if (conta == null) return (false, "Conta não encontrada.");

                // 2. Lógica de Saldo
                if (movimento.IdTipo == 1) // Receita
                {
                    conta.Montante += movimento.ValorTransacao;
                }
                else if (movimento.IdTipo == 2) // Despesa
                {
                    // Validação de Saldo Insuficiente
                    if (conta.Montante < movimento.ValorTransacao)
                        return (false, "Saldo insuficiente para realizar esta despesa.");

                    conta.Montante -= movimento.ValorTransacao;
                }
                else
                {
                    return (false, "Tipo de transação inválido.");
                }

                // 3. Persistência
                movimento.DataTransacao = DateTime.Now;
                _context.Transacaos.Add(movimento);

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return (true, "Sucesso!");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                // Isto é crucial para o teu "eu" do futuro não perder horas no debug:
                Debug.WriteLine($"[ERRO FINANCE_SERVICE]: {ex.Message}");
                Debug.WriteLine($"[STACK TRACE]: {ex.StackTrace}");

                return (false, "Erro técnico ao processar a transação. Tente novamente.");
            }
        }

        // --- DASHBOARD ---

        public async Task<decimal> GetSaldoTotalContratoAsync(int idContrato)
        {
            return await _context.Conta
                .Where(c => c.IdContrato == idContrato)
                .Select(c => (decimal?)c.Montante) // Cast para nullable para evitar erro se vazio
                .SumAsync() ?? 0m;
        }

        // 2. Implementar o método que falta para a CollectionView da Dashboard
        public async Task<List<Transacao>> GetUltimasTransacoesAsync(int idContrato)
        {
            return await _context.Transacaos
                .Include(t => t.IdCategoriaNavigation)
                .Where(t => t.IdContaNavigation!.IdContrato == idContrato)
                .OrderByDescending(t => t.DataTransacao)
                .Take(10) // Pegamos apenas as 10 últimas para a Dashboard
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetGastosPorCategoriaAsync(int idContrato)
        {
            return await _context.Transacaos
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdContaNavigation)
                .Where(t => t.IdContaNavigation!.IdContrato == idContrato && t.IdTipo == 2)
                .GroupBy(t => t.IdCategoriaNavigation!.Nome) // Substitui 'Designacao' pelo nome real que está no ficheiro Categorium.cs
                .Select(g => new {
                    // Se a categoria for nula, usamos "Outros"
                    Categoria = g.Key ?? "Outros",
                    Total = g.Sum(t => t.ValorTransacao)
                })
                .ToDictionaryAsync(x => x.Categoria, x => x.Total);
        }
    }
}