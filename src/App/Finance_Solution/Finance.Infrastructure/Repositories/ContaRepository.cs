using Dapper;
using Finance.Core.DTOs;
using Finance.Core.Interfaces;
using Finance.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly string _connectionString;
        public ContaRepository(string conn) => _connectionString = conn;

        public async Task<IEnumerable<ResumoContaDTO>> ObterResumoContasAsync()
        {
            using var db = new SqlConnection(_connectionString);
            return await db.QueryAsync<ResumoContaDTO>("SELECT * FROM v_ResumoContas");
        }

        public async Task<bool> CriarAsync(Conta c)
        {
            using var db = new SqlConnection(_connectionString);
            var sql = "INSERT INTO Conta (NomeConta, Montante, DataInicio, idContrato, IsAberta) VALUES (@NomeConta, @Montante, @DataInicio, @IdContrato, @IsAberta)";
            return await db.ExecuteAsync(sql, c) > 0;
        }
    }
}
