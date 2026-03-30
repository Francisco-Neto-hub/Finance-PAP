using System.Data;
using Microsoft.Data.SqlClient;

namespace Finance.API.Data
{
    public class FinanceContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public FinanceContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Método que devolve uma ligação aberta à base de dados
        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
