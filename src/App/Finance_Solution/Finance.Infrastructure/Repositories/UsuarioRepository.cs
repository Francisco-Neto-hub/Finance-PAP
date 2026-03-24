using Dapper;
using Finance.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;
        public UsuarioRepository(string conn) => _connectionString = conn;

        public async Task<int?> ValidarLoginAsync(string email, string password)
        {
            using var db = new SqlConnection(_connectionString);
            return await db.ExecuteScalarAsync<int?>(
                "SELECT dbo.fn_ValidarLogin(@email, @pass)", new { email, pass = password });
        }
    }
}
