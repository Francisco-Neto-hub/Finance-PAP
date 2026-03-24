using Dapper;
using Finance.Core.Interfaces;
using Finance.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Finance.Infrastructure.Repositories
{
    public class TransacaoRepository : ITransacaoRepository
    {
        private readonly string _connectionString;
        public TransacaoRepository(string conn) => _connectionString = conn;

        public async Task<bool> AdicionarAsync(int contaId, string nome, decimal valor, int catId, int tipoId)
        {
            using var db = new SqlConnection(_connectionString);
            var p = new { idConta = contaId, NomeTransacao = nome, Valor = valor, idCategoria = catId, idTipo = tipoId };
            return await db.ExecuteAsync("sp_RegistarTransacao", p, commandType: CommandType.StoredProcedure) > 0;
        }

        public async Task<bool> TransferirAsync(int o, int d, decimal v)
        {
            using var db = new SqlConnection(_connectionString);
            return await db.ExecuteAsync("sp_TransferirFundos", new { idOrigem = o, idDestino = d, Valor = v }, commandType: CommandType.StoredProcedure) > 0;
        }

        public async Task<IEnumerable<Categoria>> ListarCategoriasAsync()
        {
            using var db = new SqlConnection(_connectionString);
            return await db.QueryAsync<Categoria>("SELECT * FROM Categoria ORDER BY Nome");
        }
    }
}
