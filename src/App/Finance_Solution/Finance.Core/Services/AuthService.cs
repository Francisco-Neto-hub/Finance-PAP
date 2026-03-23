using Finance.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Finance.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceDbContext _context;

        public AuthService(FinanceDbContext context)
        {
            _context = context;
        }
        public async Task<Cliente?> ValidarCredenciaisAsync(string email, string password)
        {
            // 1. Gerar o Hash do que o utilizador escreveu (garantindo que o hash é minúsculo)
            string hashGerado = GerarHashSHA256(password).ToLower();

            // 2. Procurar o utilizador ignorando maiúsculas/minúsculas no Email (.ToLower())
            // O EF Core traduz o ToLower() para o SQL de forma eficiente
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.Trim().ToLower());

            if (cliente == null)
                return null;

            // 3. Comparação de Hash (limpando espaços e forçando minúsculas em ambos os lados)
            // Se o teu SQL for Case Sensitive, o Trim() e o ToLower() aqui salvam o dia
            bool senhaValida = cliente.ByPass.Trim().ToLower() == hashGerado;

            return senhaValida ? cliente : null;
        }

        private string GerarHashSHA256(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // "x2" gera hexadecimal em minúsculas
                }
                return builder.ToString();
            }
        }

        public async Task<bool> RegistarNovoClienteAsync(Cliente cliente)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Hash da Password
                cliente.ByPass = GerarHashSHA256(cliente.ByPass);

                // 2. Criar o Contrato (A entidade "mãe" das contas)
                var novoContrato = new Contrato
                {
                    // Preenche aqui os campos obrigatórios do teu modelo 'Contrato'
                    // Ex: NomeContrato = $"Contrato de {cliente.Nome}",
                    // DataInicio = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.Contratos.Add(novoContrato);
                await _context.SaveChangesAsync(); // Gera o IdContrato

                // 3. Criar a ligação entre Cliente e Contrato (ContratoCliente)
                var vinculo = new ContratoCliente
                {
                    IdClienteNavigation = cliente,
                    IdContratoNavigation = novoContrato
                };
                _context.ContratoClientes.Add(vinculo);

                // 4. Criar a Conta (Contum) associada a esse Contrato
                var novaConta = new Contum
                {
                    NomeConta = "Carteira Principal",
                    Montante = 0m,
                    DataInicio = DateOnly.FromDateTime(DateTime.Now),
                    IdEstadoConta = 1,
                    IdContratoNavigation = novoContrato // Ligação direta ao contrato criado acima
                };
                _context.Conta.Add(novaConta);

                // 5. Salvar tudo
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"[ERRO REGISTO]: {ex.Message}");
                return false;
            }
        }
    }
}