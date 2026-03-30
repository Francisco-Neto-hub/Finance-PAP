using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly FinanceContext _context;
        public AuditoriaController(FinanceContext context) { _context = context; }

        /// <summary>
        /// Consulta o registo de auditoria de alterações de saldos (Exclusivo para Administradores).
        /// </summary>
        /// <remarks>
        /// Este endpoint lê os dados gerados automaticamente pelos Triggers da base de dados. 
        /// Permite ao Admin ver exatamente quando um saldo mudou, o valor antigo, o valor novo e quem fez a alteração.
        /// </remarks>
        /// <returns>Lista com o histórico de alterações de saldo de todas as contas.</returns>
        [HttpGet("saldos")]
        public async Task<IActionResult> GetAuditoriaSaldos()
        {
            using var connection = _context.CreateConnection();
            var query = @"
                SELECT A.idLog, A.idConta, ISNULL(C.NomeConta, 'Conta Apagada') AS NomeConta,
                       A.SaldoAntigo, A.SaldoNovo, A.DataAlteracao, A.Usuario
                FROM Auditoria_Saldo A
                LEFT JOIN Conta C ON A.idConta = C.idConta
                ORDER BY A.DataAlteracao DESC";
            var logs = await connection.QueryAsync<AuditoriaSaldoDTO>(query);
            return Ok(logs);
        }
    }
}
