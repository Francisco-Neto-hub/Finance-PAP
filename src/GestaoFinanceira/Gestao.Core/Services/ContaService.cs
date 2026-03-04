using System;
using Dapper;
using Gestao.Core.Interfaces;
using Gestao.Core.Models;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Services
{
    public class ContaService : IContaService
    {
        private readonly DbConnectionFactory _dbFactory;

        public ContaService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public IEnumerable<Conta> ListarContasPorUtilizador(int idUtilizador)
        {
            using var db = _dbFactory.CreateConnection();
            // Mapeamento explícito para as propriedades da classe C#
            string sql = @"SELECT id_conta as IdConta, nome, saldo_inicial as SaldoInicial, 
                           id_utilizador as IdUtilizador, ativo 
                           FROM Conta WHERE id_utilizador = @idUtilizador AND ativo = 1";
            return db.Query<Conta>(sql, new { idUtilizador });
        }

        public void CriarConta(Conta conta)
        {
            using var db = _dbFactory.CreateConnection();
            string sql = @"INSERT INTO Conta (nome, saldo_inicial, id_utilizador, ativo) 
                           VALUES (@Nome, @SaldoInicial, @IdUtilizador, 1)";
            db.Execute(sql, conta);
        }

        public decimal ObterSaldoAtual(int idConta)
        {
            using var db = _dbFactory.CreateConnection();
            // Soma o saldo inicial com as entradas e subtrai as saídas
            string sql = @"
                SELECT 
                    (SELECT saldo_inicial FROM Conta WHERE id_conta = @idConta) +
                    ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = @idConta AND id_tipo_movimento = 1), 0) -
                    ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = @idConta AND id_tipo_movimento = 2), 0)";
            return db.ExecuteScalar<decimal>(sql, new { idConta });
        }
    }
}
