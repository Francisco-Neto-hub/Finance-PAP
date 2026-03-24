using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Finance.Core.Models;
using Finance.Core.Interfaces;

namespace Finance.Infrastructure.Services;

public class DataService : IDataService
{
    private readonly string _connectionString;

    public DataService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    // 1. LOGIN utilizando a Function SQL
    public async Task<int?> ValidarLoginAsync(string email, string password)
    {
        using var db = CreateConnection();
        // Chamamos a função escalar do SQL
        var sql = "SELECT dbo.fn_ValidarLogin(@email, @pass)";
        return await db.ExecuteScalarAsync<int?>(sql, new { email, pass = password });
    }

    // 2. ADICIONAR MOVIMENTO utilizando a Stored Procedure
    public async Task<bool> AdicionarMovimentoAsync(int contaId, string titulo, decimal valor, int categoriaId, int tipoId)
    {
        using var db = CreateConnection();
        var parameters = new
        {
            idConta = contaId,
            NomeTransacao = titulo,
            Valor = valor,
            idCategoria = categoriaId,
            idTipo = tipoId
        };

        try
        {
            await db.ExecuteAsync("sp_RegistarTransacao", parameters, commandType: CommandType.StoredProcedure);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // 3. CONSULTAR CONTAS utilizando a VIEW
    public async Task<IEnumerable<Conta>> GetResumoContasAsync()
    {
        using var db = CreateConnection();
        // Note: A View já faz os Joins complexos, aqui o código fica simples
        var sql = "SELECT * FROM v_ResumoContas";
        return await db.QueryAsync<Conta>(sql);
    }
}
