using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Finance.API.DTOs.RelatoriosDTO;
using Finance.API.Data;
using Finance.API.DTOs;
using System.Data;
using Dapper;

namespace Finance.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RelatoriosController : ControllerBase
    {
        private readonly FinanceContext _context;
        public RelatoriosController(FinanceContext context) { _context = context; }

        [HttpGet("extrato-detalhado")]
        public async Task<IActionResult> GetExtratoDetalhado(int mes, int ano, int? idConta = null, int? idCategoria = null, string? busca = null)
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();

            // Traz os detalhes de todas as transações daquele mês
            var query = @"
        SELECT 
            T.idTransacao, T.NomeTransacao, T.ValorTransacao, T.DataTransacao,
            Cat.Nome AS Categoria, C.NomeConta AS ContaDestino, TM.Descricao AS TipoMovimento
        FROM Transacao T
        INNER JOIN Categoria Cat ON T.idCategoria = Cat.idCategoria
        INNER JOIN Conta C ON T.idConta = C.idConta
        INNER JOIN Tipo_Movimento TM ON T.idTipo = TM.idTipo
        INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
        WHERE CC.idCliente = @IdCliente 
          AND T.IsConcluida = 1
          AND MONTH(T.DataTransacao) = @Mes 
          AND YEAR(T.DataTransacao) = @Ano
          -- FILTROS DINÂMICOS:
          AND (@IdConta IS NULL OR @IdConta = 0 OR T.idConta = @IdConta)
          AND (@IdCategoria IS NULL OR @IdCategoria = 0 OR T.idCategoria = @IdCategoria)
          AND (@Busca IS NULL OR T.NomeTransacao LIKE '%' + @Busca + '%')
        ORDER BY T.DataTransacao DESC;";

            var resultado = await connection.QueryAsync<RelatorioTransacaoDTO>(query,
                new { IdCliente = idClienteClaim, Mes = mes, Ano = ano, IdConta = idConta, IdCategoria = idCategoria, Busca = busca });

            return Ok(resultado);
        }
    }
}
