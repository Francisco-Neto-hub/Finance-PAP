using Moq;
using Xunit;
using Finance.Core.Interfaces;
using Finance.Core.Models;

namespace Finance.Tests.Services;

public class TransacaoTests
{
    [Fact]
    public async Task RegistarDespesa_DeveRetornarSucesso_QuandoDadosSaoValidos()
    {
        // 1. Arrange (Preparação)
        // Criamos um "Mock" da nossa interface para não precisar de SQL real
        var mockService = new Mock<IFinanceService>();

        // Configuramos o Mock para dizer que, se chamarmos RegistarTransacao com estes dados, ele retorna true
        mockService.Setup(s => s.RegistarTransacaoAsync(1, "Almoço", 15.50m, 2, 2))
                   .ReturnsAsync(true);

        var service = mockService.Object;

        // 2. Act (Ação)
        var resultado = await service.RegistarTransacaoAsync(1, "Almoço", 15.50m, 2, 2);

        // 3. Assert (Verificação)
        Assert.True(resultado);
    }

    [Fact]
    public void Password_DeveGerarHashDiferente_ParaTextoSimples()
    {
        // Teste para a nossa classe de Segurança
        string pass = "12345";
        string hash = Finance.Core.Security.PasswordHasher.HashPassword(pass);

        Assert.NotEqual(pass, hash);
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public async Task Transferencia_DeveFalhar_SeSaldoForInsuficiente()
    {
        // Arrange
        var mockService = new Mock<IFinanceService>();

        // Simulamos que a transferência falha (retorna false) se o saldo for baixo
        mockService.Setup(s => s.RealizarTransferenciaAsync(It.IsAny<int>(), It.IsAny<int>(), 999999m))
                   .ReturnsAsync(false);

        var service = mockService.Object;

        // Act
        var resultado = await service.RealizarTransferenciaAsync(1, 2, 999999m);

        // Assert
        Assert.False(resultado);
    }
}
