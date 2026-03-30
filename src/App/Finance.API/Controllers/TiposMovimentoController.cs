using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TiposMovimentoController : ControllerBase
    {
        private readonly FinanceContext _context;
        public TiposMovimentoController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Lista os tipos de movimentos financeiros possíveis.
        /// </summary>
        /// <remarks>
        /// Normalmente devolve: 1 - Receita, 2 - Despesa, 3 - Transferência. Utilizado para preencher as opções de criação de transação no Frontend.
        /// </remarks>
        /// <returns>Lista com os tipos de movimento.</returns>
        [HttpGet]
        public async Task<IActionResult> ListarTipos()
        {
            using var connection = _context.CreateConnection();
            var query = "SELECT idTipo, descricao FROM Tipo_Movimento ORDER BY idTipo";
            var tipos = await connection.QueryAsync<TipoMovimentoReadDTO>(query);
            return Ok(tipos);
        }
    }
}
