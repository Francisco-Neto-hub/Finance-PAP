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
    public class TransacoesController : ControllerBase
    {
        private readonly FinanceContext _context;

        public TransacoesController(FinanceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Regista uma nova transação (Receita, Despesa ou Transferência).
        /// </summary>
        /// <remarks>
        /// Garante através de Transações SQL que o saldo da conta é atualizado em simultâneo com a criação do registo. 
        /// Bloqueia despesas se o saldo for insuficiente.
        /// </remarks>
        /// <param name="request">Dados da transação a efetuar.</param>
        [HttpPost]
        public async Task<IActionResult> CriarTransacao([FromBody] TransacaoCreateDTO request)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

                if (request.ValorTransacao <= 0)
                    return BadRequest(new { mensagem = "O valor da transação tem de ser maior que zero." });

                if (request.IdTipo == 3 && request.IdContaDestino == null)
                    return BadRequest(new { mensagem = "Para transferências, tens de indicar a conta de destino." });

                using var connection = _context.CreateConnection();

                // A magia do BEGIN TRANSACTION
                var query = @"
                    BEGIN TRY
                        BEGIN TRANSACTION;

                        -- 1. Validar se a Conta de Origem pertence ao utilizador e está aberta
                        IF NOT EXISTS (
                            SELECT 1 FROM Conta C
                            INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                            WHERE C.idConta = @IdContaOrigem AND CC.idCliente = @IdCliente AND C.IsAberta = 1
                        )
                        BEGIN
                            THROW 51000, 'Conta de origem inválida, fechada ou não te pertence.', 1;
                        END

                        -- 2. Lógica por Tipo de Movimento
                        IF @IdTipo = 1 -- RECEITA (Soma o valor)
                        BEGIN
                            UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @IdContaOrigem;
                        END
                        ELSE IF @IdTipo = 2 -- DESPESA (Subtrai, mas verifica saldo)
                        BEGIN
                            DECLARE @SaldoAtual DECIMAL(18,2);
                            SELECT @SaldoAtual = Montante FROM Conta WHERE idConta = @IdContaOrigem;
                            
                            IF (@SaldoAtual - @Valor) < 0
                                THROW 51001, 'Saldo insuficiente para realizar esta despesa.', 1;

                            UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @IdContaOrigem;
                        END
                        ELSE IF @IdTipo = 3 -- TRANSFERÊNCIA (Tira de uma e põe noutra)
                        BEGIN
                            -- Validar se a Conta Destino existe e está aberta
                            IF NOT EXISTS (SELECT 1 FROM Conta WHERE idConta = @IdContaDestino AND IsAberta = 1)
                                THROW 51002, 'A conta de destino não existe ou está fechada.', 1;

                            -- Verificar saldo na origem
                            DECLARE @SaldoOrigem DECIMAL(18,2);
                            SELECT @SaldoOrigem = Montante FROM Conta WHERE idConta = @IdContaOrigem;
                            
                            IF (@SaldoOrigem - @Valor) < 0
                                THROW 51003, 'Saldo insuficiente para realizar esta transferência.', 1;

                            -- Efetuar o movimento duplo
                            UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @IdContaOrigem;
                            UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @IdContaDestino;
                        END

                        -- 3. Registar a Transação na tabela de movimentos
                        INSERT INTO Transacao (idConta, idCategoria, idTipo, ValorTransacao, NomeTransacao, DataTransacao, IsConcluida)
                        VALUES (@IdContaOrigem, @IdCategoria, @IdTipo, @Valor, @NomeTransacao, @DataTransacao, 1);

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH
                ";

                await connection.ExecuteAsync(query, new
                {
                    IdCliente = idClienteClaim,
                    IdContaOrigem = request.IdContaOrigem,
                    IdContaDestino = request.IdContaDestino,
                    IdCategoria = request.IdCategoria,
                    IdTipo = request.IdTipo,
                    Valor = request.ValorTransacao,
                    NomeTransacao = request.NomeTransacao,
                    DataTransacao = request.DataTransacao
                });

                return Ok(new { mensagem = "Transação registada com sucesso!" });
            }
            catch (Exception ex)
            {
                // Apanhamos as mensagens personalizadas geradas pelo THROW no SQL
                if (ex.Message.Contains("Saldo insuficiente") || ex.Message.Contains("Conta de origem") || ex.Message.Contains("conta de destino"))
                    return BadRequest(new { mensagem = ex.Message });

                return StatusCode(500, new { mensagem = "Erro ao registar a transação.", erro = ex.Message });
            }
        }
        /// <summary>
        /// Obtem o Extrato de uma conta específica.
        /// </summary>
        /// <remarks>
        /// Utiliza uma query para buscar as transações.
        /// </remarks>
        /// <param name="idConta">ID da conta da qual quer o extrato.</param>

        [HttpGet("conta/{idConta}")]
        public async Task<IActionResult> ObterExtrato(int idConta)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                using var connection = _context.CreateConnection();

                var query = @"
                    SELECT 
                        T.idTransacao, 
                        TM.Descricao AS TipoMovimento, 
                        C.Nome AS Categoria, 
                        T.ValorTransacao, 
                        T.NomeTransacao, 
                        T.DataTransacao, 
                        T.IsConcluida
                    FROM Transacao T
                    INNER JOIN Tipo_Movimento TM ON T.idTipo = TM.idTipo
                    INNER JOIN Categoria C ON T.idCategoria = C.idCategoria
                    INNER JOIN Conta CO ON T.idConta = CO.idConta
                    INNER JOIN Contrato_Cliente CC ON CO.idContrato = CC.idContrato
                    WHERE CC.idCliente = @IdCliente AND T.idConta = @IdConta
                    ORDER BY T.DataTransacao DESC";

                var extrato = await connection.QueryAsync<TransacaoReadDTO>(query, new { IdCliente = idClienteClaim, IdConta = idConta });
                return Ok(extrato);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao obter extrato.", erro = ex.Message });
            }
        }

        /// <summary>
        /// Obtem o Extrato de uma conta específica com filto entre datas.
        /// </summary>
        /// <remarks>
        /// Utiliza uma query para buscar as transações por datas.
        /// </remarks>
        [HttpGet("extrato/{idConta}")]
        public async Task<IActionResult> GetExtrato(int idConta, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                if (string.IsNullOrEmpty(idClienteClaim)) return Unauthorized();

                using var connection = _context.CreateConnection();

                // 1. Segurança Máxima: Garantir que a conta pertence ao utilizador que fez o pedido
                var contaValida = await connection.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(1) FROM Conta C
                    INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                    WHERE C.idConta = @IdConta AND CC.idCliente = @IdCliente",
                    new { IdConta = idConta, IdCliente = idClienteClaim });

                if (contaValida == 0)
                    return BadRequest(new { mensagem = "Acesso negado: Esta conta não existe ou não te pertence." });

                // 2. Construir a query base para buscar os movimentos e cruzar com as categorias e tipos
                var query = @"
                    SELECT 
                        T.idTransacao AS IdTransacao,
                        TM.descricao AS TipoMovimento,
                        Cat.Nome AS Categoria,
                        T.ValorTransacao,
                        T.NomeTransacao,
                        T.DataTransacao,
                        T.IsConcluida
                    FROM Transacao T
                    INNER JOIN Tipo_Movimento TM ON T.idTipo = TM.idTipo
                    INNER JOIN Categoria Cat ON T.idCategoria = Cat.idCategoria
                    WHERE T.idConta = @IdConta";

                // 3. Adicionar os filtros de data de forma dinâmica (se o utilizador os enviou)
                if (dataInicio.HasValue)
                    query += " AND T.DataTransacao >= @DataInicio";

                if (dataFim.HasValue)
                    // Adicionamos as 23:59:59 para garantir que apanha o dia todo
                    query += " AND T.DataTransacao <= @DataFimFiltro";

                query += " ORDER BY T.DataTransacao DESC";

                // Formatar a data final para o final do dia, se existir
                DateTime? dataFimFormatada = dataFim.HasValue ? dataFim.Value.Date.AddDays(1).AddTicks(-1) : null;

                var transacoes = await connection.QueryAsync<TransacaoReadDTO>(query, new
                {
                    IdConta = idConta,
                    DataInicio = dataInicio,
                    DataFimFiltro = dataFimFormatada
                });

                return Ok(transacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao obter o extrato.", erro = ex.Message });
            }
        }

        /// <summary>
        /// Anula uma transação existente e corrige o saldo da conta.
        /// </summary>
        /// <remarks>
        /// Utiliza a lógica de Soft Delete (IsConcluida = 0) para manter o rasto da operação anulada.
        /// </remarks>
        /// <param name="idTransacao">ID do movimento a anular.</param>
        [HttpPut("{idTransacao}/anular")]
        public async Task<IActionResult> AnularTransacao(int idTransacao)
        {
            try
            {
                var idClienteClaim = User.FindFirst("idCliente")?.Value;
                using var connection = _context.CreateConnection();

                var queryAnular = @"
                    BEGIN TRY
                        BEGIN TRANSACTION;

                        DECLARE @IdConta INT, @Valor DECIMAL(18,2), @IdTipo INT, @IsConcluida BIT;
                        
                        SELECT @IdConta = T.idConta, @Valor = T.ValorTransacao, @IdTipo = T.idTipo, @IsConcluida = T.IsConcluida
                        FROM Transacao T
                        INNER JOIN Conta C ON T.idConta = C.idConta
                        INNER JOIN Contrato_Cliente CC ON C.idContrato = CC.idContrato
                        WHERE T.idTransacao = @IdTransacao AND CC.idCliente = @IdCliente;

                        IF @IsConcluida = 0
                            THROW 51000, 'A transação já se encontra anulada.', 1;

                        IF @IdTipo = 3
                            THROW 51001, 'Anulação de Transferências não é suportada por esta via.', 1;

                        -- Reverter o saldo na conta
                        IF @IdTipo = 1 -- Era Receita (somou), logo agora subtrai
                            UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @IdConta;
                        ELSE IF @IdTipo = 2 -- Era Despesa (subtraiu), logo agora devolve o dinheiro
                            UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @IdConta;

                        -- Marcar como anulada (Soft Delete)
                        UPDATE Transacao SET IsConcluida = 0 WHERE idTransacao = @IdTransacao;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH
                ";

                await connection.ExecuteAsync(queryAnular, new { IdTransacao = idTransacao, IdCliente = idClienteClaim });
                return Ok(new { mensagem = "A transação foi anulada e o saldo da conta foi corrigido." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("anulada") || ex.Message.Contains("Transferências"))
                    return BadRequest(new { mensagem = ex.Message });

                return StatusCode(500, new { mensagem = "Erro ao anular a transação.", erro = ex.Message });
            }
        }
    }
}
