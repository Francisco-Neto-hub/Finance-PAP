using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Dapper;
using Gestao.Core.Services;
using Gestao.Core.Models;
using System.Linq;

namespace Gestao.Tests.Sevices
{
    public class DbIntegrationTests
    {
        private readonly DbConnectionFactory _dbFactory = new DbConnectionFactory();

        [Fact]
        public void Deve_Conectar_E_Consultar_Utilizadores_Do_Seed()
        {
            // Arrange & Act
            using var db = _dbFactory.CreateConnection();
            // Consulta os utilizadores que inseriste no script SQL (João Silva, Maria Costa)
            var users = db.Query<Utilizador>("SELECT * FROM Utilizador").ToList();

            // Assert
            Assert.NotNull(users);
            Assert.True(users.Count >= 2); // O teu script insere 2 utilizadores iniciais
            Assert.Contains(users, u => u.Nome == "João Silva");
        }

        [Fact]
        public void Deve_Validar_View_Saldo_Actual()
        {
            using var db = _dbFactory.CreateConnection();

            // Testa se a View que criaste no SQL está acessível
            var saldo = db.QueryFirst("SELECT TOP 1 saldo_actual FROM vw_SaldoActual WHERE id_conta = 1");

            Assert.NotNull(saldo);
        }
    }
}
