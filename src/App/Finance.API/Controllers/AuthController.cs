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

        /// <summary>
        /// Altera a password de um cliente.
        /// </summary>
        /// <remarks>
        /// Este endpoint realiza várias operações numa única transação SQL:
        /// 1. Verifica quem é o utilizador através do Token.
        /// 2. Faz a validação e o Update tudo numa única query SQL usando HASHBYTES.
        /// 3. Caso a password antiga esteja errada ele apanha o nosso erro personalizado do SQL.
        /// </remarks>
        /// <param name="request">Mudança de password.</param>
        /// <returns>Mensagem de sucesso.</returns>    
        [Authorize]
        [HttpPut("mudar-password")]
        public async Task<IActionResult> MudarPassword([FromBody] MudarPasswordDTO request)
        {
            try
            {
                // Descobrir quem é o utilizador através do Token
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

                using var connection = _context.CreateConnection();

                // Fazemos a validação e o Update tudo numa única query SQL usando HASHBYTES
                var query = @"
                    BEGIN TRY
                        -- 1. Verificar se a password antiga está correta
                        IF NOT EXISTS (
                            SELECT 1 FROM Cliente 
                            WHERE idCliente = @IdCliente 
                            AND PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @PasswordAntiga), 2)
                        )
                        BEGIN
                            THROW 51004, 'A password atual está incorreta.', 1;
                        END

                        -- 2. Atualizar para a nova password encriptada
                        UPDATE Cliente 
                        SET PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @PasswordNova), 2)
                        WHERE idCliente = @IdCliente;
                    END TRY
                    BEGIN CATCH
                        THROW;
                    END CATCH
                ";

                await connection.ExecuteAsync(query, new
                {
                    IdCliente = idClienteClaim,
                    PasswordAntiga = request.PasswordAntiga,
                    PasswordNova = request.PasswordNova
                });

                return Ok(new { mensagem = "Password alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                // Apanhar o nosso erro personalizado do SQL
                if (ex.Message.Contains("password atual está incorreta"))
                    return BadRequest(new { mensagem = ex.Message });

                return StatusCode(500, new { mensagem = "Erro ao alterar a password.", erro = ex.Message });
            }
        }

        /// <summary>
        /// Permite a um utilizador recuperar e alterar a sua palavra-passe caso a tenha esquecido.
        /// </summary>
        /// <remarks>
        /// Por motivos de segurança (e ausência de envio de SMS/Email no ambiente de teste), 
        /// o utilizador tem de provar a sua identidade fornecendo o Email e o Telemóvel exatos que usou no registo.
        /// </remarks>
        /// <param name="request">Dados de validação e a nova palavra-passe.</param>
        /// <returns>Mensagem de sucesso ou de erro de validação.</returns>
        [HttpPut("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordDTO request)
        {
            using var connection = _context.CreateConnection();

            // 1. Verificar se a combinação Email + Telemóvel existe na base de dados e se o cliente está ativo
            var queryVerificacao = @"
                SELECT idCliente 
                FROM Cliente 
                WHERE Email = @Email AND Telemovel = @Telemovel AND IsAtivo = 1 AND IsExcluido = 0";

            var idCliente = await connection.ExecuteScalarAsync<int?>(queryVerificacao, new
            {
                Email = request.Email,
                Telemovel = request.Telemovel
            });

            // Se devolver null, é porque tentaram adivinhar os dados e falharam (ou a conta está apagada)
            if (idCliente == null)
            {
                return BadRequest(new { mensagem = "Os dados fornecidos não correspondem a nenhuma conta ativa no sistema." });
            }

            // 2. Se os dados baterem certo, atualizamos a password e CONVERTEMOS para Hexadecimal!
            var queryAtualizar = @"
                UPDATE Cliente 
                SET PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @NovaPassword), 2)
                WHERE idCliente = @IdCliente";

            await connection.ExecuteAsync(queryAtualizar, new
            {
                NovaPassword = request.NovaPassword,
                IdCliente = idCliente
            });

            return Ok(new { mensagem = "Palavra-passe recuperada e alterada com sucesso! Já podes fazer Login." });
        }
    }    
}