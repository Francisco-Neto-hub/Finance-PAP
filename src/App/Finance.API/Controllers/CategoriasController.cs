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
    public class CategoriasController : ControllerBase
    {
        private readonly FinanceContext _context;
        public CategoriasController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Lista todas as categorias de transações disponíveis no sistema.
        /// </summary>
        /// <remarks>
        /// Ideal para popular a Dropdown (Combobox) no ecrã de "Nova Transação" do Frontend.
        /// </remarks>
        /// <returns>Lista de categorias ordenadas alfabeticamente.</returns>
        [HttpGet]
        public async Task<IActionResult> ListarCategorias()
        {
            try
            {
                using var connection = _context.CreateConnection();
                var query = "SELECT idCategoria, Nome FROM Categoria ORDER BY Nome";
                var categorias = await connection.QueryAsync<CategoriaReadDTO>(query);
                return Ok(categorias);
            }
            catch (Exception ex) { return StatusCode(500, new { erro = ex.Message }); }
        }
    }
}
