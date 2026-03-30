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
    public class DashboardController : ControllerBase
    {
        private readonly FinanceContext _context;

        public DashboardController(FinanceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todos os dados necessários para o ecrã inicial do cliente.
        /// </summary>
        /// <remarks>
        /// Devolve os saldos atuais, o total de receitas/despesas do mês, agrupamento de gastos por categoria e as últimas transações.
        /// </remarks>
        /// <returns>Objeto complexo com o resumo financeiro do cliente logado.</returns>
        [HttpGet("resumo")]
        public async Task<IActionResult> GetResumoDashboard()
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized(new { mensagem = "Token inválido." });

                using var connection = _context.CreateConnection();

                var queryBatch = @"
                    -- 1. Contas e Saldo Atual
                    SELECT C.NomeConta, C.Montante 
                    FROM Conta C
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND C.IsAberta = 1;

                    -- 2. Totais do Mês (Receitas e Despesas do mês corrente)
                    SELECT 
                        ISNULL(SUM(CASE WHEN T.idTipo = 1 THEN T.ValorTransacao ELSE 0 END), 0) AS TotalReceitasMes,
                        ISNULL(SUM(CASE WHEN T.idTipo = 2 THEN T.ValorTransacao ELSE 0 END), 0) AS TotalDespesasMes
                    FROM Transacao T
                    INNER JOIN Conta C ON T.idConta = C.idConta
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND T.IsConcluida = 1 
                    AND MONTH(T.DataTransacao) = MONTH(GETDATE()) AND YEAR(T.DataTransacao) = YEAR(GETDATE());

                    -- 3. Gastos por Categoria (Apenas Despesas do mês corrente)
                    SELECT Cat.Nome AS Categoria, SUM(T.ValorTransacao) AS TotalGasto
                    FROM Transacao T
                    INNER JOIN Categoria Cat ON T.idCategoria = Cat.idCategoria
                    INNER JOIN Conta C ON T.idConta = C.idConta
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND T.idTipo = 2 AND T.IsConcluida = 1
                    AND MONTH(T.DataTransacao) = MONTH(GETDATE()) AND YEAR(T.DataTransacao) = YEAR(GETDATE())
                    GROUP BY Cat.Nome;

                    -- 4. Últimas 5 Transações
                    SELECT TOP 5 
                        T.NomeTransacao, T.ValorTransacao, T.DataTransacao, TM.Descricao AS TipoMovimento
                    FROM Transacao T
                    INNER JOIN Tipo_Movimento TM ON T.idTipo = TM.idTipo
                    INNER JOIN Conta C ON T.idConta = C.idConta
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND T.IsConcluida = 1
                    ORDER BY T.DataTransacao DESC;
                ";

                using var multi = await connection.QueryMultipleAsync(queryBatch, new { IdCliente = idClienteClaim });

                var dashboardData = new DashboardResponseDTO();

                // Ler Resultado 1: Contas e Saldo Total
                dashboardData.Contas = (await multi.ReadAsync<ContaResumoDTO>()).ToList();
                dashboardData.SaldoTotal = dashboardData.Contas.Sum(c => c.Montante);

                // Ler Resultado 2: Totais do Mês
                var totais = await multi.ReadSingleAsync<dynamic>();
                dashboardData.TotalReceitasMes = (decimal)totais.TotalReceitasMes;
                dashboardData.TotalDespesasMes = (decimal)totais.TotalDespesasMes;

                // Ler Resultado 3: Gastos por Categoria
                dashboardData.GastosPorCategoria = (await multi.ReadAsync<GastoCategoriaDTO>()).ToList();

                // Ler Resultado 4: Últimas Transações
                dashboardData.UltimasTransacoes = (await multi.ReadAsync<TransacaoRecenteDTO>()).ToList();

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao carregar o dashboard.", erro = ex.Message });
            }
        }
    }
}
