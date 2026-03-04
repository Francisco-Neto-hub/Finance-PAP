using Gestao.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Tests
{
    public class VersaoServiceTest
    {
        [Fact]
        public void VerificarVersao_DeveRetornarVersaoDoSQL()
        {
            // Arrange
            var factory = new DbConnectionFactory();
            var service = new VersionService(factory);

            // Act
            var resultado = service.ObterUltimaVersao();

            // Assert
            Assert.True(resultado != null, "A tabela Versoes_Software parece estar vazia no SQL Server.");
            Assert.Equal("1.0.0", resultado.Versao);
        }
    }
}
