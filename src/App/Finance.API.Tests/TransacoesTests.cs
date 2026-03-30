using Finance.API.Controllers;
using Finance.API.DTOs;
using Finance.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Finance.API.Tests
{
    public class TransacoesTests
    {
        // NOTA: Para este teste funcionar, precisas de colocar a tua connection string real de desenvolvimento
        // ou de uma base de dados de testes para o Dapper conseguir executar a query.
        private readonly string _connectionString = "Server=DESKTOP-76S1NRV\\SQLEXPRESS;Database=BD_Finance;Trusted_Connection=True;TrustServerCertificate=True;";

        [Fact] // O [Fact] diz ao Visual Studio que isto é um teste para correr
        public async Task CriarTransacao_SaldoInsuficiente_DeveDevolverBadRequest()
        {
            // ARRANGE (Preparação)
            // 1. Configurar o contexto da base de dados (simular o IConfiguration)
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    {"ConnectionStrings:DefaultConnection", _connectionString}
                })
                .Build();

            var context = new FinanceContext(configuration);
            var controller = new TransacoesController(context);

            // 2. Simular um utilizador logado com o Token (IdCliente = 1)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("idCliente", "1")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // 3. Montar o pedido: Tentar transferir 1 Milhão de Euros!
            var request = new TransacaoCreateDTO
            {
                IdContaOrigem = 1, // Substitui por um ID de conta real que não tenha 1 milhão
                IdContaDestino = 1010, // Substitui por um ID de conta destino real
                IdCategoria = 4,
                IdTipo = 3, // Transferência
                ValorTransacao = 1000000.00m, // Valor absurdo
                NomeTransacao = "Tentativa de Fraude"
            };

            // ACT (Ação - Vamos chamar o endpoint!)
            var resultado = await controller.CriarTransacao(request);

            // ASSERT (Verificação)
            // O resultado ESPERADO é que a API nos devolva um erro "400 BadRequest" com a mensagem de saldo.
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);

            // Vamos ler a mensagem que veio no JSON do erro
            var json = badRequestResult.Value?.ToString();
            Assert.Contains("Saldo insuficiente", json);
        }

        [Fact]
        public async Task GetExtrato_ContaNaoPertenceAoCliente_DeveDevolverBadRequest()
        {
            // ARRANGE
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    {"ConnectionStrings:DefaultConnection", _connectionString}
                }).Build();

            var context = new FinanceContext(configuration);
            var controller = new TransacoesController(context);

            // Simular o "Frank" logado (IdCliente = 12)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("idCliente", "12")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // ACT
            // O Frank vai tentar espiar a conta com o ID 9999 (que não lhe pertence ou não existe)
            int contaDaMaria = 9999;
            var resultado = await controller.GetExtrato(contaDaMaria, null, null);

            // ASSERT
            // Esperamos que a API o bloqueie e devolva um BadRequest (400) com mensagem de acesso negado
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
            var json = badRequestResult.Value?.ToString();

            Assert.Contains("Acesso negado", json);
        }
    }
}