using Finance.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Interfaces
{
    public interface ITransacaoRepository
    {
        Task<bool> AdicionarAsync(int contaId, string nome, decimal valor, int categoriaId, int tipoId);
        Task<bool> TransferirAsync(int origemId, int destinoId, decimal valor);
        Task<IEnumerable<Categoria>> ListarCategoriasAsync();
    }
}
