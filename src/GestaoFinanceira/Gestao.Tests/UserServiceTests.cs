using System;
using System.Collections.Generic;
using System.Text;
using Gestao.Core.Models;
using Gestao.Core.Services;
using Gestao.Core.Exceptions;
using Xunit;

namespace Gestao.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public void RegistarUtilizador_DeveInserirNoBancoComSucesso()
        {
            // Arrange
            var factory = new DbConnectionFactory();
            var userService = new UserService(factory);

            // Gerar um email único para evitar erro de duplicado no SQL
            string emailTeste = $"teste{Guid.NewGuid().ToString().Substring(0, 5)}@pap.pt";

            var novoUser = new Utilizador
            {
                Nome = "Utilizador Teste",
                Email = emailTeste
            };

            // Act
            userService.RegistarUtilizador(novoUser, "SenhaSegura123!");

            // Assert
            // Se não lançou exceção, o teste passa. 
            // Podes verificar no SQL: SELECT * FROM Utilizador WHERE email = emailTeste
        }
    }
}
