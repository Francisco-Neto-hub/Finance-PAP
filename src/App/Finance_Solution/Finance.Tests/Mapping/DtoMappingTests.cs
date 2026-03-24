using Finance.Core.DTOs;
using Xunit;

namespace Finance.Tests.Mapping;

public class DtoMappingTests
{
    [Fact]
    public void ResumoContaDTO_DeveReterValoresCorretamente()
    {
        // Arrange
        var dataAtual = DateTime.Now;

        // Act
        var dto = new ResumoContaDTO
        {
            IdConta = 1,
            NomeConta = "Conta Poupança",
            SaldoAtual = 1500.75m,
            Estado = "Ativa",
            Titular = "Francisco",
            DataInicio = dataAtual
        };

        // Assert
        Assert.Equal(1500.75m, dto.SaldoAtual);
        Assert.Equal("Ativa", dto.Estado);
        Assert.Equal("Francisco", dto.Titular);
    }
}
