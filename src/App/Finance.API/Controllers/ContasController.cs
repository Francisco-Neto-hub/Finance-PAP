using Microsoft.AspNetCore.Http;
using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    // Apenas utilizadores com login feito podem aceder
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContasController : ControllerBase
    {
        private readonly FinanceContext _context;

        public ContasController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Lista todas as contas ativas do utilizador autenticado.
        /// </summary>
        /// <returns>Lista ideal para preencher as Dropdowns (Combobox) no Frontend.</returns>
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetMinhasContas()
        {
            var idClienteClaim = User.FindFirst("idCliente")?.Value;
            if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

            using var connection = _context.CreateConnection();
            var query = @"
                    SELECT 
                    C.idConta as Id, 
                    C.NomeConta,       -- (Antes estava C.NomeConta as Nome)
                    C.Montante         -- (Antes estava C.Montante as Saldo)
                FROM Conta C
                INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                WHERE CC.idCliente = @IdCliente AND C.IsAberta = 1";

            var contas = await connection.QueryAsync(query, new { IdCliente = idClienteClaim });
            return Ok(contas);
        }

        /// <summary>
        /// Cria uma nova conta bancária associada ao contrato do utilizador.
        /// </summary>
        /// <param name="request">Nome e montante inicial da nova conta.</param>
        /// <returns>Mensagem de sucesso.</returns>
        [HttpPost("nova_conta")]
        public async Task<IActionResult> CriarConta([FromBody] ContaUserCreateDTO request)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

                using var connection = _context.CreateConnection();
                var queryContrato = "SELECT TOP 1 idContrato FROM Contrato_Cliente WHERE idCliente = @IdCliente";
                var idContrato = await connection.ExecuteScalarAsync<int?>(queryContrato, new { IdCliente = idClienteClaim });

                if (idContrato == null) return BadRequest(new { mensagem = "Contrato não encontrado." });

                var queryConta = @"
                    INSERT INTO Conta (idContrato, NomeConta, Montante, IsAberta, DataInicio) 
                    VALUES (@IdContrato, @NomeConta, @Montante, 1, GETDATE())";

                await connection.ExecuteAsync(queryConta, new { IdContrato = idContrato, NomeConta = request.NomeConta, Montante = request.Montante });
                return Ok(new { mensagem = "Conta criada com sucesso!" });
            }
            catch (Exception ex) { return StatusCode(500, new { mensagem = "Erro ao criar conta.", erro = ex.Message }); }
        }

        /// <summary>
        /// Encerra uma conta bancária (Soft Delete).
        /// </summary>
        /// <remarks>
        /// A conta não é apagada fisicamente para manter o histórico, mas o seu estado passa a 'IsAberta = 0'.
        /// </remarks>
        /// <param name="idConta">ID da conta a ser encerrada.</param>
        [HttpPut("{idConta}/fechar_conta")]
        public async Task<IActionResult> FecharConta(int idConta)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                using var connection = _context.CreateConnection();
                var queryClose = @"
                    UPDATE C SET C.IsAberta = 0 FROM Conta C
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND C.idConta = @IdConta";

                var linhas = await connection.ExecuteAsync(queryClose, new { IdCliente = idClienteClaim, IdConta = idConta });
                if (linhas == 0) return NotFound(new { mensagem = "Conta não encontrada." });
                return Ok(new { mensagem = "Conta fechada com sucesso." });
            }
            catch (Exception ex) { return StatusCode(500, new { mensagem = "Erro ao fechar conta.", erro = ex.Message }); }
        }
    }
}
