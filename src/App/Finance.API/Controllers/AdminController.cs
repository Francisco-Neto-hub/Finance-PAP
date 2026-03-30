using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace Finance.API.Controllers
{
    // A tag de segurança máxima: Só quem tem o IdPerfil correspondente a "Admin" entra aqui.
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly FinanceContext _context;
        public AdminController(FinanceContext context) { _context = context; }

        #region GESTÃO DE CLIENTES

        /// <summary>
        /// Lista todos os clientes registados na plataforma (Modo Administrador).
        /// </summary>
        /// <returns>Lista global de clientes com os respetivos estados de conta e perfis.</returns>
        [HttpGet("listar_clientes")]
        public async Task<IActionResult> ListarClientesGlobais()
        {
            using var connection = _context.CreateConnection();
            var query = @"
                SELECT idCliente, nome, email, telemovel, DataNasc, idPerfil, IsAtivo, IsExcluido 
                FROM Cliente ORDER BY nome";
            var clientes = await connection.QueryAsync<ClienteAdminReadDTO>(query);
            return Ok(clientes);
        }

        /// <summary>
        /// Altera o perfil de permissões de um cliente (ex: promover um Cliente Normal a Administrador).
        /// </summary>
        /// <param name="idCliente">ID do cliente alvo.</param>
        /// <param name="novoIdPerfil">ID numérico do novo perfil a atribuir.</param>
        /// <returns>Mensagem de sucesso na atualização de privilégios.</returns>
        [HttpPut("clientes/{idCliente}/perfil")]
        public async Task<IActionResult> PromoverCliente(int idCliente, [FromBody] int novoIdPerfil)
        {
            using var connection = _context.CreateConnection();
            var perfilExiste = await connection.ExecuteScalarAsync<bool>(
                "SELECT CAST(COUNT(1) AS BIT) FROM Perfil WHERE idPerfil = @IdPerfil", new { IdPerfil = novoIdPerfil });

            if (!perfilExiste) return BadRequest(new { mensagem = "Perfil inexistente." });

            var linhas = await connection.ExecuteAsync("UPDATE Cliente SET idPerfil = @IdPerfil WHERE idCliente = @IdCliente",
                new { IdPerfil = novoIdPerfil, IdCliente = idCliente });

            if (linhas == 0) return NotFound("Cliente não encontrado.");
            return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
        }

        /// <summary>
        /// Bloqueia ou desbloqueia o acesso de um cliente à plataforma.
        /// </summary>
        /// <remarks>
        /// Se o novo estado for definido como falso (IsAtivo = 0), o cliente ficará totalmente impedido de fazer login no sistema.
        /// </remarks>
        /// <param name="idCliente">ID do cliente alvo.</param>
        /// <param name="request">Objeto indicando o novo estado (true para ativo, false para bloqueado).</param>
        /// <returns>Mensagem indicando a nova situação do acesso do cliente.</returns>
        [HttpPut("clientes/{idCliente}/estado")]
        public async Task<IActionResult> BloquearDesbloquearCliente(int idCliente, [FromBody] AlterarEstadoDTO request)
        {
            using var connection = _context.CreateConnection();
            // IsAtivo = 0 impede o cliente de fazer Login
            var linhas = await connection.ExecuteAsync("UPDATE Cliente SET IsAtivo = @Estado WHERE idCliente = @IdCliente",
                new { Estado = request.NovoEstado, IdCliente = idCliente });

            if (linhas == 0) return NotFound("Cliente não encontrado.");
            string accao = request.NovoEstado ? "desbloqueado" : "bloqueado";
            return Ok(new { mensagem = $"O acesso do cliente foi {accao} com sucesso." });
        }

        #endregion

        #region GESTÃO DE CONTAS (GLOBAL)

        /// <summary>
        /// Lista todas as contas bancárias registadas na plataforma.
        /// </summary>
        /// <remarks>
        /// Permite ao Administrador ver os saldos, o estado atual das contas (aberta/fechada) e identificar os respetivos titulares. A lista é apresentada por ordem decrescente de saldo (os mais ricos primeiro).
        /// </remarks>
        /// <returns>Lista de todas as contas existentes no banco.</returns>
        [HttpGet("contas")]
        public async Task<IActionResult> ListarContasGlobais()
        {
            using var connection = _context.CreateConnection();
            var query = @"
                SELECT C.idConta, C.NomeConta, C.Montante, C.IsAberta, Cl.Nome AS NomeCliente
                FROM Conta C
                INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                INNER JOIN Cliente Cl ON CC.idCliente = Cl.idCliente
                ORDER BY C.Montante DESC"; // Mostra os mais ricos primeiro!
            var contas = await connection.QueryAsync<ContaGlobalReadDTO>(query);
            return Ok(contas);
        }

        /// <summary>
        /// Congela ou reabre uma conta bancária específica.
        /// </summary>
        /// <remarks>
        /// Congelar uma conta impede novas transações e o seu uso geral, mas mantém o histórico e o saldo intactos na base de dados (Soft Delete na flag IsAberta).
        /// </remarks>
        /// <param name="idConta">ID da conta bancária a alterar.</param>
        /// <param name="request">Objeto indicando o novo estado da conta (true para reabrir, false para congelar).</param>
        /// <returns>Mensagem confirmando se a conta foi congelada ou reaberta com sucesso.</returns>
        [HttpPut("contas/{idConta}/estado")]
        public async Task<IActionResult> CongelarConta(int idConta, [FromBody] AlterarEstadoDTO request)
        {
            using var connection = _context.CreateConnection();
            var linhas = await connection.ExecuteAsync("UPDATE Conta SET IsAberta = @Estado WHERE idConta = @IdConta",
                new { Estado = request.NovoEstado, IdConta = idConta });

            if (linhas == 0) return NotFound("Conta não encontrada.");
            string accao = request.NovoEstado ? "reaberta" : "congelada";
            return Ok(new { mensagem = $"A conta foi {accao} com sucesso." });
        }

        #endregion

        #region GESTÃO DE TRANSAÇÕES E AUDITORIA

        /// <summary>
        /// Lista todas as transações de todos os clientes do banco.
        /// </summary>
        /// <remarks>
        /// Ferramenta de auditoria global para o Administrador visualizar o fluxo de capital de toda a plataforma em tempo real.
        /// </remarks>
        /// <returns>Lista detalhada de transações globais ordenadas da mais recente para a mais antiga.</returns>
        [HttpGet("transacoes")]
        public async Task<IActionResult> ListarTransacoesGlobais()
        {
            using var connection = _context.CreateConnection();
            var query = @"
                SELECT T.idTransacao, Cl.Nome AS NomeCliente, C.NomeConta, TM.descricao AS TipoMovimento, 
                       T.ValorTransacao, T.DataTransacao, T.IsConcluida
                FROM Transacao T
                INNER JOIN Conta C ON T.idConta = C.idConta
                INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                INNER JOIN Cliente Cl ON CC.idCliente = Cl.idCliente
                INNER JOIN Tipo_Movimento TM ON T.idTipo = TM.idTipo
                ORDER BY T.DataTransacao DESC";
            var transacoes = await connection.QueryAsync<TransacaoGlobalReadDTO>(query);
            return Ok(transacoes);
        }

        /// <summary>
        /// Força a anulação de uma transação e corrige automaticamente o saldo da conta afetada.
        /// </summary>
        /// <remarks>
        /// O Administrador pode usar este endpoint para reverter despesas ou receitas fraudulentas ou erradas, ignorando as restrições de propriedade do cliente. Utiliza transações SQL (BEGIN TRANSACTION) para garantir a integridade dos fundos.
        /// </remarks>
        /// <param name="idTransacao">ID da transação a ser forçadamente anulada.</param>
        /// <returns>Mensagem de sucesso com a confirmação da correção do saldo.</returns>
        [HttpPut("transacoes/{idTransacao}/forcar-anulacao")]
        public async Task<IActionResult> ForcarAnulacaoAdmin(int idTransacao)
        {
            // O Admin pode anular qualquer transação, não verifica se lhe pertence!
            using var connection = _context.CreateConnection();
            var query = @"
                BEGIN TRY
                    BEGIN TRANSACTION;
                    
                    DECLARE @IdConta INT, @Valor DECIMAL(18,2), @IdTipo INT, @Concluida BIT;
                    SELECT @IdConta = idConta, @Valor = ValorTransacao, @IdTipo = idTipo, @Concluida = IsConcluida 
                    FROM Transacao WHERE idTransacao = @IdTransacao;

                    IF @Concluida = 0 THROW 51000, 'Esta transação já está anulada.', 1;
                    IF @IdTipo = 3 THROW 51000, 'Transferências têm de ser revertidas manualmente.', 1;

                    IF @IdTipo = 1 UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @IdConta;
                    IF @IdTipo = 2 UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @IdConta;

                    UPDATE Transacao SET IsConcluida = 0 WHERE idTransacao = @IdTransacao;

                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    ROLLBACK TRANSACTION;
                    THROW;
                END CATCH
            ";
            try
            {
                await connection.ExecuteAsync(query, new { IdTransacao = idTransacao });
                return Ok(new { mensagem = "Transação anulada à força pelo Administrador. Saldo corrigido." });
            }
            catch (Exception ex) { return BadRequest(new { erro = ex.Message }); }
        }

        #endregion

        #region GESTÃO DE CATEGORIAS DO SISTEMA

        /// <summary>
        /// Cria uma nova categoria de transações no sistema.
        /// </summary>
        /// <param name="request">Objeto contendo o nome da nova categoria.</param>
        /// <returns>Mensagem de sucesso indicando que a categoria foi criada.</returns>
        [HttpPost("categorias")]
        public async Task<IActionResult> CriarCategoria([FromBody] CategoriaAdminDTO request)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync("INSERT INTO Categoria (Nome) VALUES (@Nome)", new { Nome = request.Nome });
            return Ok(new { mensagem = "Categoria criada!" });
        }

        /// <summary>
        /// Atualiza o nome de uma categoria existente.
        /// </summary>
        /// <param name="idCategoria">ID da categoria a ser alterada.</param>
        /// <param name="request">Objeto contendo o novo nome da categoria.</param>
        /// <returns>Mensagem confirmando a atualização com sucesso.</returns>
        [HttpPut("categorias/{idCategoria}")]
        public async Task<IActionResult> AtualizarCategoria(int idCategoria, [FromBody] CategoriaAdminDTO request)
        {
            using var connection = _context.CreateConnection();
            var linhas = await connection.ExecuteAsync("UPDATE Categoria SET Nome = @Nome WHERE idCategoria = @Id",
                new { Nome = request.Nome, Id = idCategoria });
            if (linhas == 0) return NotFound();
            return Ok(new { mensagem = "Categoria atualizada!" });
        }

        /// <summary>
        /// Elimina fisicamente uma categoria do sistema.
        /// </summary>
        /// <remarks>
        /// Se a categoria já possuir transações associadas, a API bloqueia a eliminação para proteger a integridade referencial.
        /// </remarks>
        /// <param name="idCategoria">ID da categoria a apagar.</param>
        [HttpDelete("categorias/{idCategoria}")]
        public async Task<IActionResult> ApagarCategoriaAdmin(int idCategoria)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var linhas = await connection.ExecuteAsync("DELETE FROM Categoria WHERE idCategoria = @IdCategoria", new { IdCategoria = idCategoria });
                if (linhas == 0) return NotFound();
                return Ok(new { mensagem = "Categoria eliminada." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint"))
                    return BadRequest(new { mensagem = "Não podes apagar esta categoria porque está em uso por clientes!" });
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        #endregion
    }
}
