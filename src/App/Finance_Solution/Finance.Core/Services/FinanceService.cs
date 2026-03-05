using Finance.Core.Data;
using Finance.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Finance.Core.Services
{
    public class FinanceService
    {
        private readonly FinanceDbContext _context;

        public FinanceService(FinanceDbContext context)
        {
            _context = context;
        }

        // Obtém todas as contas de um contrato específico
        public async Task<List<Conta>> GetContasPorContratoAsync(int idContrato)
        {
            return await _context.Contas
                .Where(c => c.IdContrato == idContrato)
                .ToListAsync();
        }

        // Regista um movimento e atualiza o saldo automaticamente
        public async Task<bool> RegistarMovimentoAsync(Transacao novaTransacao)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var conta = await _context.Contas.FindAsync(novaTransacao.IdConta);
                if (conta == null) return false;

                // 1 = Receita (Soma), 2 = Despesa (Subtrai)
                if (novaTransacao.IdTipo == 1)
                    conta.Montante += novaTransacao.ValorTransacao;
                else if (novaTransacao.IdTipo == 2)
                    conta.Montante -= novaTransacao.ValorTransacao;

                _context.Transacoes.Add(novaTransacao);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
