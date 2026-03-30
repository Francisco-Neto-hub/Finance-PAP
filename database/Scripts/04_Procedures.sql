/* Procedures */

/* 1. Stored Procedure:
   Automatiza o processo: ela insere o movimento e atualiza o saldo da conta 
   de uma só vez. Isto evita erros de sincronização de dados. */

CREATE PROCEDURE sp_RegistarTransacao
    @idConta INT,
    @NomeTransacao VARCHAR(100),
    @Valor DECIMAL(18,2),
    @idCategoria INT,
    @idTipo INT -- 1 para Receita, 2 para Despesa
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Inserir a transação
        INSERT INTO [Transacao] (idConta, NomeTransacao, ValorTransacao, idCategoria, idTipo, IsConcluida)
        VALUES (@idConta, @NomeTransacao, @Valor, @idCategoria, @idTipo, 1);

        -- 2. Atualizar o montante na conta
        IF @idTipo = 1 -- Receita (Soma)
        BEGIN
            UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @idConta;
        END
        ELSE IF @idTipo = 2 -- Despesa (Subtrai)
        BEGIN
            UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @idConta;
        END

        COMMIT TRANSACTION;
        PRINT 'Transação realizada e saldo atualizado com sucesso!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT 'Erro ao realizar transação.';
    END CATCH
END;

/* 2. Procedure de Transferência (Entre Contas) */
GO
CREATE PROCEDURE sp_TransferirFundos
    @idContaOrigem INT,
    @idContaDestino INT,
    @Valor DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validar se há saldo suficiente na origem
    IF (SELECT Montante FROM Conta WHERE idConta = @idContaOrigem) < @Valor
    BEGIN
        RAISERROR('Saldo insuficiente para a transferência.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Retirar da conta de origem
        UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @idContaOrigem;
        
        -- 2. Adicionar na conta de destino
        UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @idContaDestino;

        -- 3. Registar as transações para histórico (Opcional, mas recomendado)
        INSERT INTO [Transacao] (idConta, NomeTransacao, ValorTransacao, idTipo, IsConcluida)
        VALUES (@idContaOrigem, 'Transferência Enviada', @Valor, 2, 1); -- Tipo 2: Despesa
        
        INSERT INTO [Transacao] (idConta, NomeTransacao, ValorTransacao, idTipo, IsConcluida)
        VALUES (@idContaDestino, 'Transferência Recebida', @Valor, 1, 1); -- Tipo 1: Receita

        COMMIT TRANSACTION;
        PRINT 'Transferência realizada com sucesso!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        RAISERROR('Erro ao processar a transferência.', 16, 1);
    END CATCH
END;
GO

/* 3. Soft Delete */
-- 1. Alterar a tabela (Primeiro bloco)
ALTER TABLE Cliente ADD IsExcluido BIT DEFAULT 0;
GO -- <--- Este comando separa os lotes

-- 2. Criar a Procedure (Segundo bloco - Agora ela é a primeira do lote)
CREATE PROCEDURE sp_ApagarCliente
    @idCliente INT
AS
BEGIN
    SET NOCOUNT ON; -- Boa prática para performance
    
    -- Em vez de DELETE, fazemos UPDATE (Soft Delete)
    UPDATE Cliente 
    SET IsExcluido = 1, IsAtivo = 0 
    WHERE idCliente = @idCliente;
    
    PRINT 'Cliente desativado e marcado como excluído (os dados históricos foram preservados).';
END;
GO