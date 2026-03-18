using Finance.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Finance.Core.Services
{
    public class AuthService
    {
        private readonly FinanceDbContext _context;

        public AuthService(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ValidarCredenciaisAsync(string email, string password)
        {
            // O Scaffold costuma gerar nomes em PascalCase (ex: ByPass em vez de by_pass)
            return await _context.Clientes
                .Include(c => c.IdEstadoClienteNavigation) // Nome gerado pelo Scaffold para a relação
                .FirstOrDefaultAsync(c => c.Email == email &&
                                          c.ByPass == password &&
                                          c.IdEstadoCliente == 1);
        }

        // Novo Método de Registo adaptado ao teu Model "Cliente"
        public async Task<bool> RegistarClienteAsync(Cliente novoCliente)
        {
            try
            {
                bool existe = await _context.Clientes
                    .AnyAsync(c => c.Email.ToLower() == novoCliente.Email.ToLower());

                if (existe) return false;

                // Agora isto já funciona!
                novoCliente.IdEstadoCliente = 1;
                novoCliente.DataCriacao = DateTime.Now;

                _context.Clientes.Add(novoCliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO]: {ex.Message}");
                return false;
            }
        }
    }
}