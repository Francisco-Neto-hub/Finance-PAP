using Microsoft.EntityFrameworkCore;
using Finance.Core.Services;
using Finance.Core.Models;

namespace Finance.Tests
{
    public class AuthTests
    {
        private FinanceDbContext GetDbContext()
        {
            var connectionString = "Server=DESKTOP-76S1NRV\\SQLEXPRESS;Database=Finance_BD_v2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
            var options = new DbContextOptionsBuilder<FinanceDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            return new FinanceDbContext(options);
        }

        [Theory]
        [InlineData("francisco@email.com", "12345")] // Dados que confirmámos existirem na BD
        public async Task ValidarLoginComDadosReais_Scaffold(string email, string pass)
        {
            // ARRANGE
            using var context = GetDbContext();
            var authService = new AuthService(context);

            // ACT
            var resultado = await authService.ValidarCredenciaisAsync(email, pass);

            // ASSERT
            // 1. Verificar se o serviço não retornou nulo

            // 2. Verificar se o email retornado é o correto
            Assert.Equal(email, resultado!.Email);

            // 3. Verificar se o estado do cliente é Ativo (Id 1)
            Assert.Equal(1, resultado.IdEstadoCliente);
        }

        [Fact]
        public async Task Login_Com_Password_Errada_Deve_Retornar_Null()
        {
            // ARRANGE
            using var context = GetDbContext();
            var authService = new AuthService(context);

            // ACT
            var resultado = await authService.ValidarCredenciaisAsync("francisco@email.com", "password_errada_999");

            // ASSERT
            Assert.Null(resultado);
        }
    }
}
