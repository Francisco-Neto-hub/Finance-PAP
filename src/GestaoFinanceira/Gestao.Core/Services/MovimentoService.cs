using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Gestao.Core.Interfaces;
using Gestao.Core.Models;

namespace Gestao.Core.Services
{
    public class MovimentoService : IMovimentoService
    {
        private readonly DbConnectionFactory _dbFactory;

        public MovimentoService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public void RegistarMovimento(Movimento movimento)
        {
            using var db = _dbFactory.CreateConnection();
            string sql = @"INSERT INTO Movimento (data, valor, descricao, id_conta, id_categoria, id_tipo_movimento) 
                           VALUES (@Data, @Valor, @Descricao, @IdConta, @IdCategoria, @IdTipoMovimento)";

            db.Execute(sql, movimento);
        }

        public IEnumerable<Movimento> ListarPorConta(int idConta)
        {
            using var db = _dbFactory.CreateConnection();
            // Aqui podes filtrar pelo campo 'ativo' que adicionaste no script
            return db.Query<Movimento>("SELECT * FROM Movimento WHERE id_conta = @idConta AND ativo = 1", new { idConta });
        }

        public void DesativarMovimento(int idMovimento)
        {
            using var db = _dbFactory.CreateConnection();
            db.Execute("UPDATE Movimento SET ativo = 0 WHERE id_movimento = @idMovimento", new { idMovimento });
        }
    }
}
