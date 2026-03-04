using System;
using Dapper;
using Gestao.Core.Interfaces;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Gestao.Core.Services
{
    public class AlertService : IAlertService
    {
        private readonly DbConnectionFactory _dbFactory;

        public AlertService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // IMPLEMENTAÇÃO DO MÉTODO QUE ESTAVA A FALTAR
        public IEnumerable<string> ObterAlertasAtivos(int idUtilizador)
        {
            using var db = _dbFactory.CreateConnection();

            // Query que faz o JOIN entre as tabelas do teu diagrama
            string sql = @"
                SELECT an.descricao 
                FROM Alertas_Notificacoes an
                INNER JOIN Utilizador_Alertas ua ON an.id_alerta = ua.id_alerta
                WHERE ua.id_utilizador = @idUtilizador
                ORDER BY ua.data_criacao DESC";

            return db.Query<string>(sql, new { idUtilizador });
        }

        public void CriarAlerta(int idUtilizador, int idConta, string descricao)
        {
            using var db = _dbFactory.CreateConnection();

            // 1. Inserir a notificação (Tabela Alertas_Notificacoes)
            string sqlAlerta = "INSERT INTO Alertas_Notificacoes (descricao) OUTPUT INSERTED.id_alerta VALUES (@descricao)";
            int idAlerta = db.QuerySingle<int>(sqlAlerta, new { descricao });

            // 2. Associar ao utilizador (Tabela Utilizador_Alertas)
            string sqlUserAlerta = @"INSERT INTO Utilizador_Alertas (id_utilizador, id_conta, id_alerta, data_criacao) 
                                     VALUES (@idUtilizador, @idConta, @idAlerta, GETDATE())";

            db.Execute(sqlUserAlerta, new { idUtilizador, idConta, idAlerta });
        }
    }
}
