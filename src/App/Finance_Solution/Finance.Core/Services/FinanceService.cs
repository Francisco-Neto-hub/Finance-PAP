using Finance.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; // Necessário para o Debug.WriteLine

namespace Finance.Core.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly FinanceDbContext _context;

        public FinanceService(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Sucesso, string Mensagem)> RegistarTransacaoAsync(Transacao movimento)
        {
            if (movimento.ValorTransacao <= 0)
                return (false, "O valor da transação deve ser superior a zero.");

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var conta = await _context.Conta.FindAsync(movimento.IdConta);
                if (conta == null) return (false, "Conta não encontrada.");

                // Lógica de Saldo
                if (movimento.IdTipo == 1) // Receita
                {
                    conta.Montante = (conta.Montante ?? 0) + movimento.ValorTransacao;
                }
                else if (movimento.IdTipo == 2) // Despesa
                {
                    if ((conta.Montante ?? 0) < movimento.ValorTransacao)
                        return (false, "Saldo insuficiente na conta.");

                    conta.Montante = (conta.Montante ?? 0) - movimento.ValorTransacao;
                }

                _context.Transacaos.Add(movimento);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return (true, "Transação registada com sucesso!");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                Debug.WriteLine($"[ERRO TRANSACAO]: {ex.Message}");
                return (false, "Erro técnico ao registar transação.");
            }
        }

        public async Task<decimal> GetSaldoTotalContratoAsync(int idContrato)
        {
            // O uso de (decimal?) evita erro se não houver contas
            return await _context.Conta
                .Where(c => c.IdContrato == idContrato)
                .Select(c => (decimal?)c.Montante)
                .SumAsync() ?? 0m;
        }

        public async Task<List<Transacao>> GetUltimasTransacoesAsync(int idContrato)
        {
            return await _context.Transacaos
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdContaNavigation)
                .Where(t => t.IdContaNavigation != null && t.IdContaNavigation.IdContrato == idContrato)
                .OrderByDescending(t => t.DataTransacao)
                .Take(10)
                .ToListAsync() ?? new List<Transacao>();
        }

        public async Task<Dictionary<string, decimal>> GetGastosPorCategoriaAsync(int idContrato)
        {
            var dados = await _context.Transacaos
                .Include(t => t.IdCategoriaNavigation)
                .Include(t => t.IdContaNavigation)
                .Where(t => t.IdContaNavigation != null &&
                            t.IdContaNavigation.IdContrato == idContrato &&
                            t.IdTipo == 2) // Apenas Despesas
                .GroupBy(t => t.IdCategoriaNavigation != null ? t.IdCategoriaNavigation.Nome : "Outros")
                .Select(g => new { Categoria = g.Key, Total = g.Sum(t => t.ValorTransacao) })
                .ToListAsync();

            return dados.ToDictionary(x => x.Categoria, x => x.Total);
        }
    }
}