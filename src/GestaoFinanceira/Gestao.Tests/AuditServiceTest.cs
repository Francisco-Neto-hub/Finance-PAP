using Gestao.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace Gestao.Tests
{
    public class AuditServiceTest
    {
        [Fact]
        public void AlterarMovimento_DeveGerarRegistoNoHistorico()
        {
            // Arrange
            var factory = new DbConnectionFactory();
            var service = new AuditService(factory);

            // Act
            service.RegistarAlteracao(1, "valor", "50.00", "100.00", 1);

            // Assert
            using var db = factory.CreateConnection();
            var gravado = db.QueryFirstOrDefault<int>("SELECT COUNT(1) FROM Historico WHERE id_movimento = 1");
            Assert.True(gravado > 0);
        }
    }
}
