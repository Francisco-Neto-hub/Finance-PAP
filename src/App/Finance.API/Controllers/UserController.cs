using Finance.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly FinanceContext _context;
        public UserController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Permite ao utilizador autenticado eliminar a sua própria conta (Soft Delete).
        /// </summary>
        /// <remarks>
        /// Ao invocar este endpoint, a conta não é apagada fisicamente (para preservar o histórico financeiro). 
        /// É executada a Stored Procedure 'sp_ApagarCliente' que marca o cliente como excluído e impede futuros logins.
        /// </remarks>
        /// <returns>Mensagem de despedida e confirmação da eliminação lógica.</returns>
        [HttpDelete("apagar")]
        public async Task<IActionResult> ApagarConta()
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

                using var connection = _context.CreateConnection();
                // O teu Soft Delete excelente!
                await connection.ExecuteAsync("EXEC sp_ApagarCliente @idCliente", new { idCliente = idClienteClaim });

                return Ok(new { mensagem = "Conta eliminada com sucesso. Lamentamos ver-te partir!" });
            }
            catch (Exception ex) { return StatusCode(500, new { erro = ex.Message }); }
        }
    }
}
