using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Finance.Core.Data;
using Finance.Core.Services;
using Finance.Core.Models;

namespace Finance.Tests
{
    public class AuthTests
    {
        private FinanceDbContext GetDbContext()
        {
            // String de conexão que definimos anteriormente
            var connectionString = "Server=192.168.1.199;Database=Finance_BD_v2;User Id=admin;Password=812876;TrustServerCertificate=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<FinanceDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new FinanceDbContext(options);
        }

        [Fact]
        public async Task TestarConexaoEAppLogin()
        {
            // ARRANGE (Preparar)
            using var context = GetDbContext();
            var authService = new AuthService(context);

            // ACT (Executar)
            // Vamos tentar ligar à BD. Se a conexão falhar, o teste rebenta aqui.
            bool podeConectar = await context.Database.CanConnectAsync(TestContext.Current.CancellationToken);

            // ASSERT (Verificar)
            Assert.True(podeConectar, "Não foi possível conectar à base de dados no IP 192.168.1.199");
        }

        [Theory]
        [InlineData("francisco@email.com", "12345")] // Usa o email que o teste encontrou
        public async Task ValidarLoginComDadosReais(string email, string pass)
        {
            using var context = GetDbContext();
            var authService = new AuthService(context);

            // 1. Diagnóstico: Listar todos os emails para o Output do teste se falhar
            var todosEmails = await context.Clientes.Select(c => c.Email).ToListAsync(cancellationToken: TestContext.Current.CancellationToken);

            // 2. Tentar encontrar de forma flexível
            var utilizadorExiste = todosEmails.Any(e => e.Trim().ToLower() == email.Trim().ToLower());

            Assert.True(utilizadorExiste, $"Email não encontrado. Emails na BD: {string.Join(", ", todosEmails)}");

            var resultado = await authService.ValidarCredenciaisAsync(email, pass);
            Assert.NotNull(resultado);
        }
    }
}
