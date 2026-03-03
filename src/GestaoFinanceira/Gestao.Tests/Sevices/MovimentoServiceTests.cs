using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Gestao.Core.Models;

namespace Gestao.Tests.Sevices
{
    public class MovimentoServiceTests
    {
        [Fact]
        public void Deve_Criar_Instancia_De_Movimento_Com_Sucesso()
        {
            // Arrange (Preparar)
            var movimento = new Movimento
            {
                IdMovimento = 1,
                Valor = 150.00m,
                Descricao = "Teste de Integração"
            };

            // Assert (Verificar)
            Assert.Equal(150.00m, movimento.Valor);
            Assert.True(movimento.Ativo);
        }
    }
}
