using System;
using System.Collections.Generic;
using System.Text;
using Finance.Core.Models;

namespace Finance.Core.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Valida as credenciais do utilizador comparando o hash da password fornecida.
        /// </summary>
        Task<Cliente?> ValidarCredenciaisAsync(string email, string password);

        /// <summary>
        /// Regista um novo cliente, criando automaticamente um contrato e uma conta principal.
        /// </summary>
        Task<bool> RegistarNovoClienteAsync(Cliente cliente);
    }
}
