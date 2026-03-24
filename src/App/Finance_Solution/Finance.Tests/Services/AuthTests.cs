using Moq;
using Xunit;
using Finance.Core.Interfaces;
using Finance.Core.Security;

namespace Finance.Tests.Services;

public class AuthTests
{
    [Fact]
    public async Task Login_DeveRetornarIdUsuario_QuandoCredenciaisForemValidas()
    {
        // Arrange
        var mockService = new Mock<IFinanceService>();
        string email = "admin@finance.com";
        string senhaAberta = "12345";
        string senhaHash = PasswordHasher.HashPassword(senhaAberta);
        int idEsperado = 1;

        // Simulamos que a função do SQL (fn_ValidarLogin) encontrou o user e retornou o ID 1
        mockService.Setup(s => s.LoginAsync(email, senhaHash))
                   .ReturnsAsync(idEsperado);

        var service = mockService.Object;

        // Act
        var resultado = await service.LoginAsync(email, senhaHash);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(idEsperado, resultado);
    }

    [Fact]
    public async Task Login_DeveRetornarNull_QuandoPasswordForIncorreta()
    {
        // Arrange
        var mockService = new Mock<IFinanceService>();
        string email = "admin@finance.com";
        string senhaErradaHash = PasswordHasher.HashPassword("errada");

        // Simulamos que o SQL não encontrou correspondência e retornou null
        mockService.Setup(s => s.LoginAsync(email, senhaErradaHash))
                   .ReturnsAsync((int?)null);

        var service = mockService.Object;

        // Act
        var resultado = await service.LoginAsync(email, senhaErradaHash);

        // Assert
        Assert.Null(resultado);
    }
}