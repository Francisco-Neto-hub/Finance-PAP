using Dapper;
using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static Finance.API.DTOs.GraficosDTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Finance.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GraficosController : Controller
    {
        private readonly FinanceContext _context;
        public GraficosController(FinanceContext context) { _context = context; }

        [HttpGet("despesas-categoria")]
        public async Task<IActionResult> GetGastosPorCategoria([FromQuery] int mes, [FromQuery] int ano, int? idConta = null)
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();

            // Esta query agrupa por categoria e soma os valores apenas das DESPESAS (tipo 2)
            var query = @"
        SELECT Cat.Nome AS Categoria, SUM(T.ValorTransacao) AS TotalGasto
        FROM Transacao T
        INNER JOIN Categoria Cat ON T.idCategoria = Cat.idCategoria
        INNER JOIN Conta C ON T.idConta = C.idConta
        INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
        WHERE CC.idCliente = @IdCliente 
          AND T.IsConcluida = 1 
          AND T.idTipo = 2 -- 2 = Despesa (Ajusta se o teu ID for outro)
          AND MONTH(T.DataTransacao) = @Mes 
          AND YEAR(T.DataTransacao) = @Ano
          AND (@IdConta IS NULL OR @IdConta = 0 OR T.idConta = @IdConta) -- Novo Filtro!
        GROUP BY Cat.Nome;";

            var resultado = await connection.QueryAsync<DTOs.GastoCategoriaDTO>(query, new { IdCliente = idClienteClaim, Mes = mes, Ano = ano, IdConta = idConta });
            return Ok(resultado);
        }

        [HttpGet("fluxo-caixa")]
        public async Task<IActionResult> GetFluxoCaixa(int ano, int? idConta = null)
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();

            var query = @"
        SELECT 
            MONTH(T.DataTransacao) AS Mes,
            -- AQUI ESTÁ A CORREÇÃO: Os nomes (Aliases) têm de ser IGUAIS ao DTO
            SUM(CASE WHEN T.idTipo = 1 THEN T.ValorTransacao ELSE 0 END) AS TotalReceitas,
            SUM(CASE WHEN T.idTipo = 2 THEN T.ValorTransacao ELSE 0 END) AS TotalDespesas
        FROM Transacao T
        INNER JOIN Conta C ON T.idConta = C.idConta
        INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
        WHERE CC.idCliente = @IdCliente 
          AND T.IsConcluida = 1 
          AND YEAR(T.DataTransacao) = @Ano
          AND (@IdConta IS NULL OR @IdConta = 0 OR T.idConta = @IdConta)
        GROUP BY MONTH(T.DataTransacao)
        ORDER BY Mes;";

            try
            {
                var resultado = await connection.QueryAsync<FluxoCaixaDTO>(query, new { IdCliente = idClienteClaim, Ano = ano, IdConta = idConta });
                return Ok(resultado);
            }

            catch (Exception ex) 
            {
                return StatusCode(500, new { mensagem = "Erro ao carregar fluxo de caixa.", erro = ex.Message });
            }
        }
    }
}
