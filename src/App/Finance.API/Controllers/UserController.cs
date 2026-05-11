using Dapper;
using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Finance.API.DTOs.UserDTO;

namespace Finance.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly FinanceContext _context;
        public UserController(FinanceContext context) { _context = context; }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> ObterPerfil()
        {
            try
            {
                // 1. Extrair o ID do utilizador do Token
                var idCliente = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idCliente)) return Unauthorized();

                using var connection = _context.CreateConnection();

                // 2. Buscar os dados na BD
                var query = "SELECT Nome, Email, Telemovel, DataNasc FROM Cliente WHERE idCliente = @Id";
                var usuario = await connection.QueryFirstOrDefaultAsync<UserUpdateDTO>(query, new { Id = idCliente });

                if (usuario == null) return NotFound(new { mensagem = "Utilizador não encontrado." });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao obter dados do perfil.", erro = ex.Message });
            }
        }

        // 1. ALTERAR DADOS DO PERFIL
        [Authorize]
        [HttpPut("atualizar-perfil")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] UserUpdateDTO request)
        {
            try
            {
                var idCliente = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idCliente)) return Unauthorized();

                using var connection = _context.CreateConnection();

                var query = @"
                UPDATE Cliente 
                SET Nome = @Nome, 
                    Email = @Email, 
                    Telemovel = @Telemovel, 
                    DataNasc = @DataNasc 
                WHERE idCliente = @Id";

                await connection.ExecuteAsync(query, new
                {
                    request.Nome,
                    request.Email,
                    request.Telemovel,
                    request.DataNasc,
                    Id = idCliente
                });

                return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao atualizar perfil.", erro = ex.Message });
            }
        }

        // 2. MUDAR PASSWORD (DENTRO DA APP)
        [Authorize]
        [HttpPut("mudar-password")]
        public async Task<IActionResult> MudarPassword([FromBody] MudarPasswordDTO request)
        {
            try
            {
                var idCliente = User.FindFirst("idCliente")?.Value;
                using var connection = _context.CreateConnection();

                // Verificar se a password antiga está correta
                var checkPassQuery = "SELECT CAST(COUNT(1) AS BIT) FROM Cliente WHERE idCliente = @Id AND PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Antiga), 2)";
                var passCorreta = await connection.ExecuteScalarAsync<bool>(checkPassQuery, new { Id = idCliente, Antiga = request.PasswordAntiga });

                if (!passCorreta)
                    return BadRequest(new { mensagem = "A password atual está incorreta." });

                // Atualizar para a nova
                var updateQuery = "UPDATE Cliente SET PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Nova), 2) WHERE idCliente = @Id";
                await connection.ExecuteAsync(updateQuery, new { Nova = request.PasswordNova, Id = idCliente });

                return Ok(new { mensagem = "Password alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao mudar password.", erro = ex.Message });
            }
        }

        // 3. RECUPERAR PASSWORD (FORA DA APP - ESQUECI-ME)
        [AllowAnonymous] // Importante: aqui o user não tem Token ainda!
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordDTO request)
        {
            try
            {
                using var connection = _context.CreateConnection();

                // Verificar se Email e Telemovel coincidem
                var userQuery = "SELECT idCliente FROM Cliente WHERE Email = @Email AND Telemovel = @Telemovel";
                var idCliente = await connection.QueryFirstOrDefaultAsync<int?>(userQuery, new { request.Email, request.Telemovel });

                if (idCliente == null)
                    return BadRequest(new { mensagem = "Dados inválidos. Não foi possível validar a identidade." });

                // Atualizar a password
                var updateQuery = "UPDATE Cliente SET PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Nova), 2) WHERE idCliente = @Id";
                await connection.ExecuteAsync(updateQuery, new { Nova = request.NovaPassword, Id = idCliente });

                return Ok(new { mensagem = "Nova password configurada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao recuperar password.", erro = ex.Message });
            }
        }
    }
}
