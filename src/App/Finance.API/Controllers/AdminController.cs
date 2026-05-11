using Dapper;
using Finance.API.Data;
using Finance.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Finance.API.DTOs.AdminDTO;

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

        #region DASHBOARD E ESTATÍSTICAS

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            using var connection = _context.CreateConnection();
            var query = @"
        SELECT 
            (SELECT COUNT(*) FROM Cliente WHERE IsExcluido = 0) AS TotalClientes,
            (SELECT COUNT(*) FROM Cliente WHERE IsAtivo = 1 AND IsExcluido = 0) AS ClientesAtivos,
            (SELECT ISNULL(SUM(ValorTransacao), 0) FROM Transacao 
             WHERE MONTH(DataTransacao) = MONTH(GETDATE()) AND YEAR(DataTransacao) = YEAR(GETDATE()) AND IsConcluida = 1) AS VolumeTransacoesMes,
            (SELECT ISNULL(SUM(Montante), 0) FROM Conta WHERE IsAberta = 1) AS CapitalTotalCustodia";

            var stats = await connection.QueryFirstAsync<DashboardStatsDTO>(query);
            return Ok(stats);
        }

        #endregion

        #region GESTÃO DE SUPORTE

        [HttpGet("tickets-pendentes")]
        public async Task<IActionResult> ListarTicketsPendentes()
        {
            using var connection = _context.CreateConnection();
            var query = @"
        SELECT S.idTicket, C.Nome AS NomeCliente, S.Assunto, S.Mensagem, S.DataCriacao, S.IsResolvido
        FROM Suporte_Ticket S
        INNER JOIN Cliente C ON S.idCliente = C.idCliente
        WHERE S.IsResolvido = 0
        ORDER BY S.DataCriacao ASC";

            var tickets = await connection.QueryAsync<TicketSuporteDTO>(query);
            return Ok(tickets);
        }

        [HttpPut("tickets/{idTicket}/resolver")]
        public async Task<IActionResult> ResolverTicket(int idTicket)
        {
            using var connection = _context.CreateConnection();
            var linhas = await connection.ExecuteAsync(
                "UPDATE Suporte_Ticket SET IsResolvido = 1 WHERE idTicket = @Id", new { Id = idTicket });

            if (linhas == 0) return NotFound();
            return Ok(new { mensagem = "Ticket marcado como resolvido." });
        }

        #endregion

        #region GESTÃO DE CLIENTES

        /// <summary>
        /// Lista todos os clientes registados na plataforma (Modo Administrador).
        /// </summary>
        /// <returns>Lista global de clientes com os respetivos estados de conta e perfis.</returns>
        [HttpGet("listar_clientes")]
        public async Task<IActionResult> ListarClientesGlobais([FromQuery] string search = "")
        {
            using var connection = _context.CreateConnection();
            var query = @"
        SELECT idCliente, nome, email, telemovel, DataNasc, idPerfil, IsAtivo, IsExcluido 
        FROM Cliente 
        WHERE (nome LIKE @Search OR email LIKE @Search)
        ORDER BY nome";

            var clientes = await connection.QueryAsync<ClienteAdminReadDTO>(query, new { Search = $"%{search}%" });
            return Ok(clientes);
        }

        [HttpPut("clientes/{idCliente}/atualizar_dados")]
        public async Task<IActionResult> AtualizarCliente(int idCliente, [FromBody] ClienteAdminUpdateDTO request)
        {
            using var connection = _context.CreateConnection();

            // 1. Verificar se o email já existe noutro utilizador para evitar duplicados
            var emailExiste = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Cliente WHERE Email = @Email AND idCliente <> @Id",
                new { Email = request.Email, Id = idCliente });

            if (emailExiste > 0) return BadRequest(new { mensagem = "Este email já está a ser usado por outro utilizador." });

            // 2. Construir a query dinamicamente (se houver password, atualizamos, se não, mantemos)
            string query = @"
                            UPDATE Cliente 
                            SET Nome = @Nome, 
                                Email = @Email, 
                                Telemovel = @Telemovel, 
                                DataNasc = @DataNasc 
                            WHERE idCliente = @IdCliente";

            var parametros = new
            {
                Nome = request.Nome,
                Email = request.Email,
                Telemovel = request.Telemovel,
                DataNasc = request.DataNasc,
                IdCliente = idCliente
            };

            await connection.ExecuteAsync(query, parametros);

            // 3. Lógica específica para a Password (usando a tua implementação SHA2_256)
            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordQuery = "UPDATE Cliente SET PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Nova), 2) WHERE idCliente = @Id";
                await connection.ExecuteAsync(passwordQuery, new { Nova = request.Password, Id = idCliente });
            }          

            return Ok(new { mensagem = "Dados do cliente atualizados com sucesso!" });
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
            // 1. Obter o ID do Administrador que está a fazer a ação (através do Token JWT)
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (adminIdClaim != null && int.Parse(adminIdClaim) == idCliente)
            {
                return BadRequest(new { mensagem = "Protocolo de Segurança: Não podes bloquear a tua própria conta de administrador!" });
            }

            using var connection = _context.CreateConnection();

            // 2. Executar a atualização
            var query = "UPDATE Cliente SET IsAtivo = @Estado WHERE idCliente = @IdCliente AND IsExcluido = 0";
            var linhas = await connection.ExecuteAsync(query, new { Estado = request.NovoEstado, IdCliente = idCliente });

            if (linhas == 0)
                return NotFound(new { mensagem = "Cliente não encontrado ou já removido do sistema." });

            // 3. Resposta detalhada
            string accao = request.NovoEstado ? "REATIVADO" : "BLOQUEADO";

            return Ok(new
            {
                mensagem = $"Acesso {accao} com sucesso.",
                idCliente = idCliente,
                novoEstado = request.NovoEstado
            });
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
