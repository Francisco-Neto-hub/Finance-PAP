using Finance.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PerfisController : ControllerBase
    {
        private readonly FinanceContext _context;
        public PerfisController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Lista os perfis de acesso disponíveis no sistema (Exclusivo para Administradores).
        /// </summary>
        /// <remarks>
        /// Utilizado no Backoffice quando o Admin quer alterar os privilégios de um utilizador (ex: de Cliente para Admin).
        /// </remarks>
        /// <returns>Lista de perfis (Roles).</returns>
        [HttpGet]
        public async Task<IActionResult> ListarPerfis()
        {
            using var connection = _context.CreateConnection();
            var perfis = await connection.QueryAsync("SELECT idPerfil, Descricao FROM Perfil ORDER BY idPerfil");
            return Ok(perfis);
        }
    }
}
