using Gestao.Core.Models;
using Gestao.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Tests
{
    public class ContaServiceTest
    {
        [Fact]
        public void CriarConta_DeveInserirNoSQLComNomesCorretos()
        {
            // Arrange
            var factory = new DbConnectionFactory();
            var service = new ContaService(factory);
            var novaConta = new Conta
            {
                Nome = "Conta Poupança",
                SaldoInicial = 100.00m,
                IdUtilizador = 1 // Garante que este ID existe no teu SQL
            };

            // Act
            service.CriarConta(novaConta);

            // Assert
            var contas = service.ListarContasPorUtilizador(1);
            Assert.Contains(contas, c => c.Nome == "Conta Poupança");
        }
    }
}
