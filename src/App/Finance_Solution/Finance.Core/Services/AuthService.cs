using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Finance.Core.Data;
using Finance.Core.Models;

namespace Finance.Core.Services
{
    public class AuthService
    {
        private readonly FinanceDbContext _context;

        public AuthService(FinanceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Valida as credenciais do utilizador
        /// </summary>
        /// <returns>Retorna o Cliente se as credenciais forem válidas e o estado for 'Ativo' (ID 1)</returns>
        // Altera apenas para teste:
        public async Task<Cliente?> ValidarCredenciaisAsync(string email, string password)

        {
            try
            {
                // Procuramos o cliente pelo email

                var cliente = await _context.Clientes
                .Include(c => c.Estado)
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

                if (cliente == null)

                    return null; // Utilizador não encontrado


                // Verificamos a password e se o cliente está ativo (Estado ID 1 = Ativo)

                if (cliente.by_pass == password && cliente.IdEstadoCliente == 1)
                {
                    return cliente;
                }
                return null; // Password errada ou conta inativa
            }
            catch (Exception ex)
            {
                // Aqui poderias logar o erro de conexão à BD (ex: IP inacessível)

                throw new Exception("Erro ao conectar à base de dados para login.", ex);
            }
        }
    }
}
