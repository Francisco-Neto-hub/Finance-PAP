using System;
using Dapper;
using Gestao.Core.Interfaces;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly DbConnectionFactory _dbFactory;

        public AuditService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public void RegistarAlteracao(int idMovimento, string coluna, string antigo, string novo, int idUser)
        {
            using var db = _dbFactory.CreateConnection();
            string sql = @"INSERT INTO Historico (id_movimento, data_alteracao, coluna_alterada, valor_antigo, valor_novo, id_utilizador) 
                           VALUES (@idMovimento, GETDATE(), @coluna, @antigo, @novo, @idUser)";

            db.Execute(sql, new { idMovimento, coluna, antigo, novo, idUser });
        }
    }
}
