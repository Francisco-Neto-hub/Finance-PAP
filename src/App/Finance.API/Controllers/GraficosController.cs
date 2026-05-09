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
        public async Task<IActionResult> GetGastosPorCategoria([FromQuery] int mes, [FromQuery] int ano)
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();

            // Esta query agrupa por categoria e soma os valores apenas das DESPESAS (tipo 2)
            var query = @"
            SELECT 
                Cat.Nome as Categoria, 
                SUM(T.ValorTransacao) as TotalGasto
            FROM Transacao T
            INNER JOIN Categoria Cat ON T.idCategoria = Cat.idCategoria
            INNER JOIN Conta C ON T.idConta = C.idConta            
            INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
            WHERE CC.idCliente = @IdCliente 
              AND MONTH(T.DataTransacao) = @Mes 
              AND YEAR(T.DataTransacao) = @Ano
              AND T.idTipo = 2 -- Assumindo que 2 é Despesa
              AND T.IsConcluida = 1
            GROUP BY Cat.Nome";

            var resultado = await connection.QueryAsync<DTOs.GastoCategoriaDTO>(query, new { IdCliente = idClienteClaim, Mes = mes, Ano = ano });
            return Ok(resultado);
        }

        [HttpGet("fluxo-caixa")]
        public async Task<IActionResult> GetFluxoCaixa(int ano)
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();

            // A Query usa uma CTE para garantir que devolve sempre os 12 meses, 
            // preenchendo com zeros os meses sem movimentos.
            var query = @"
        WITH Meses AS (
            SELECT 1 AS Mes, 'Jan' AS NomeMes UNION ALL
            SELECT 2, 'Fev' UNION ALL
            SELECT 3, 'Mar' UNION ALL
            SELECT 4, 'Abr' UNION ALL
            SELECT 5, 'Mai' UNION ALL
            SELECT 6, 'Jun' UNION ALL
            SELECT 7, 'Jul' UNION ALL
            SELECT 8, 'Ago' UNION ALL
            SELECT 9, 'Set' UNION ALL
            SELECT 10, 'Out' UNION ALL
            SELECT 11, 'Nov' UNION ALL
            SELECT 12, 'Dez'
        ),
        TransacoesUsuario AS (
            SELECT 
                MONTH(T.DataTransacao) AS Mes,
                T.idTipo,
                T.ValorTransacao
            FROM Transacao T
            INNER JOIN Conta C ON T.idConta = C.idConta
            INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
            WHERE CC.idCliente = @IdCliente 
              AND YEAR(T.DataTransacao) = @Ano
              AND T.IsConcluida = 1
        )
        SELECT 
            M.Mes, 
            M.NomeMes,
            ISNULL(SUM(CASE WHEN TU.idTipo = 1 THEN TU.ValorTransacao ELSE 0 END), 0) AS TotalReceitas,
            ISNULL(SUM(CASE WHEN TU.idTipo = 2 THEN TU.ValorTransacao ELSE 0 END), 0) AS TotalDespesas
        FROM Meses M
        LEFT JOIN TransacoesUsuario TU ON M.Mes = TU.Mes
        GROUP BY M.Mes, M.NomeMes
        ORDER BY M.Mes;";

            try
            {
                var resultado = await connection.QueryAsync<FluxoCaixaDTO>(query, new { IdCliente = idClienteClaim, Ano = ano });
                return Ok(resultado);
            }

            catch (Exception ex) 
            {
                return StatusCode(500, new { mensagem = "Erro ao carregar fluxo de caixa.", erro = ex.Message });
            }
        }
    }
}
