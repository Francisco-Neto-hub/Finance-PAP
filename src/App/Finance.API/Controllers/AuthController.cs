using Dapper;
using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Finance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly FinanceContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(FinanceContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Autentica um cliente no sistema.
        /// </summary>
        /// <param name="request">Credenciais de acesso (Email e Password).</param>
        /// <returns>Devolve um Token JWT válido se as credenciais estiverem corretas.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                using var connection = _context.CreateConnection();

                // AJUSTE AQUI: Convertemos a password que vem do request para Hash antes de comparar
                var query = @"
                    SELECT idCliente, IdPerfil 
                    FROM Cliente 
                    WHERE Email = @Email 
                AND PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Password), 2)
                AND IsAtivo = 1 
                AND IsExcluido = 0";

                var user = await connection.QuerySingleOrDefaultAsync<dynamic>(query, new
                {
                    Email = request.Email,
                    Password = request.Password // O Dapper envia o texto limpo, o SQL faz o Hash
                });

                if (user == null) return Unauthorized(new { mensagem = "Email ou password incorretos." });

                // GERAÇÃO DO TOKEN JWT
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, request.Email),
                    new Claim("idCliente", user.idCliente.ToString()),
            
                // CORRIGIDO: IdPerfil com 'I' maiúsculo!
                    new Claim(ClaimTypes.Role, user.IdPerfil == 1 ? "Admin" : "Cliente")
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiracao = token.ValidTo,

                    // CORRIGIDO: IdPerfil com 'I' maiúsculo!
                    perfil = user.IdPerfil == 1 ? "Admin" : "Cliente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao efetuar login.", erro = ex.Message });
            }
        }

        /// <summary>
        /// Regista um novo cliente na plataforma.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza várias operações numa única transação SQL:
        /// 1. Cria o Cliente.
        /// 2. Cria o Contrato.
        /// 3. Associa o Cliente ao Contrato.
        /// 4. Abre a primeira Conta à Ordem com saldo zero.
        /// </remarks>
        /// <param name="request">Dados pessoais do novo cliente.</param>
        /// <returns>Mensagem de sucesso.</returns>
        [HttpPost("registar")]
        public async Task<IActionResult> RegistarCliente([FromBody] RegistoRequestDTO request)
            {
            try
            {
                using var connection = _context.CreateConnection();

                var checkEmailQuery = "SELECT CAST(COUNT(1) AS BIT) FROM Cliente WHERE Email = @Email";
                var emailExiste = await connection.ExecuteScalarAsync<bool>(checkEmailQuery, new { Email = request.Email });
                if (emailExiste) return BadRequest(new { mensagem = "Este email já está registado no sistema." });

                var queryRegisto = @"
                BEGIN TRY
                BEGIN TRANSACTION;

                -- 1. Inserir Cliente (Aqui está a magia do SHA-256!)
                INSERT INTO Cliente (Nome, Email, Telemovel, DataNasc, PasswordHash, IdPerfil, IsAtivo, IsExcluido, DataCriacao)
                VALUES (@Nome, @Email, @Telemovel, @DataNasc, CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Password), 2), 2, 1, 0, GETDATE());                
                DECLARE @NovoIdCliente INT = SCOPE_IDENTITY(); 

                -- 2. Inserir Contrato 
                INSERT INTO Contrato (DataInicio, IsVigente) 
                VALUES (GETDATE(), 1);
                
                DECLARE @NovoIdContrato INT = SCOPE_IDENTITY(); 

                -- 3. Ligar Contrato ao Cliente 
                INSERT INTO Contrato_Cliente (idContrato, idCliente, IsTitular)
                VALUES (@NovoIdContrato, @NovoIdCliente, 1);

                -- 4. Criar a primeira Conta bancária
                INSERT INTO Conta (idContrato, NomeConta, Montante, IsAberta, DataInicio) 
                VALUES (@NovoIdContrato, 'Conta à Ordem Principal', 0, 1, GETDATE());

                COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                ROLLBACK TRANSACTION;
                THROW;
                END CATCH
                ";

                await connection.ExecuteAsync(queryRegisto, new
                {
                    Nome = request.Nome,
                    Email = request.Email,
                    Telemovel = request.Telemovel,
                    DataNasc = request.DataNasc,
                    Password = request.Password // O C# manda limpo, o SQL Server encarrega-se de encriptar!
                });

                return Ok(new { mensagem = "Cliente registado e contrato criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao registar o cliente.", erro = ex.Message });
            }
        }        
    }    
}