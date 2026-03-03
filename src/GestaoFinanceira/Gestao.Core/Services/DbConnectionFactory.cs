using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Gestao.Core.Services
{
    public class DbConnectionFactory
    {
        // Altera 'TEU_SERVIDOR' pelo nome da tua instância SQL (ex: . ou (localdb)\MSSQLLocalDB)
        private readonly string _connectionString = "Server=DESKTOP-QMNRH7F\\SQLEXPRESS;Database=BD_Finance_v1;Trusted_Connection=True;TrustServerCertificate=True;";

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
