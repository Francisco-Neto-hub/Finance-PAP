/* PROJETO: Finance - Sistema de Gestão de Contas (Versão Final Otimizada)
   AUTORES: Francisco Neto e Francisco Loureiro
   DATA: 2026
   DESCRIÇÃO: Script unificado com estados em BIT, Soft Delete, Procedures e Views.
*/

USE master;
GO
-- Criar base de dados se não existir (Opcional, podes comentar se já tiveres a BD criada)
-- CREATE DATABASE BD_Finance;
-- GO
-- USE BD_Finance;
-- GO

-- ==========================================================
-- 1. ESTRUTURA DAS TABELAS (DDL)
-- ==========================================================

CREATE TABLE Categoria (
    idCategoria INT PRIMARY KEY IDENTITY(1,1),
    Nome VARCHAR(50) NOT NULL
);

CREATE TABLE Tipo_Movimento (
    idTipo INT PRIMARY KEY IDENTITY(1,1),
    descricao VARCHAR(20) NOT NULL 
);

CREATE TABLE Perfil (
    IdPerfil INT PRIMARY KEY IDENTITY(1,1),
    NomePerfil VARCHAR(50) NOT NULL,
    Descricao VARCHAR(255)
);

CREATE TABLE Cliente (
    idCliente INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(100) NOT NULL,
    telemovel VARCHAR(20),
    email VARCHAR(100) UNIQUE NOT NULL,
    DataNasc DATE,
    IsAtivo BIT DEFAULT 1,
    IsExcluido BIT DEFAULT 0, -- Soft Delete
    IdPerfil INT NOT NULL DEFAULT 2,
    PasswordHash VARCHAR(255) NOT NULL DEFAULT '12345',
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IdPerfil) REFERENCES Perfil(IdPerfil)
);

CREATE TABLE Contrato (
    idContrato INT PRIMARY KEY IDENTITY(1,1),
    DataInicio DATE NOT NULL,
    DataFim DATE,
    IsVigente BIT DEFAULT 1
);

CREATE TABLE Contrato_Cliente (
    idContrato INT,
    idCliente INT,
    IsTitular BIT DEFAULT 1,
    PRIMARY KEY (idContrato, idCliente),
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato),
    FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente)
);

CREATE TABLE Conta (
    idConta INT PRIMARY KEY IDENTITY(1,1),
    NomeConta VARCHAR(100) NOT NULL,
    IsAberta BIT DEFAULT 1,
    Montante DECIMAL(18,2) DEFAULT 0.00,
    DataInicio DATE NOT NULL,
    DataFim DATE,
    idContrato INT,
    CONSTRAINT CHK_DatasConta CHECK (DataFim IS NULL OR DataFim >= DataInicio),
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato)
);

CREATE TABLE [Transacao] (
    idTransacao INT PRIMARY KEY IDENTITY(1,1),
    idConta INT NOT NULL,
    IsConcluida BIT DEFAULT 1,
    NomeTransacao VARCHAR(100) NOT NULL,
    DataTransacao DATETIME NOT NULL DEFAULT GETDATE(),
    ValorTransacao DECIMAL(18,2) NOT NULL,
    idCategoria INT,
    idTipo INT,     
    FOREIGN KEY (idConta) REFERENCES Conta(idConta),
    FOREIGN KEY (idCategoria) REFERENCES Categoria(idCategoria),
    FOREIGN KEY (idTipo) REFERENCES Tipo_Movimento(idTipo)
);

CREATE TABLE Auditoria_Saldo (
    idLog INT PRIMARY KEY IDENTITY(1,1),
    idConta INT,
    SaldoAntigo DECIMAL(18,2),
    SaldoNovo DECIMAL(18,2),
    DataAlteracao DATETIME DEFAULT GETDATE(),
    Usuario VARCHAR(50) -- Pode ser o e-mail do admin que fez a alteração
);
GO

-- ==========================================================
-- 2. DADOS INICIAIS (SEED DATA)
-- ==========================================================

INSERT INTO Tipo_Movimento (descricao) VALUES ('Receita'), ('Despesa'), ('Transferência');
INSERT INTO Perfil (NomePerfil, Descricao) VALUES 
('Admin', 'Acesso total'), ('Utilizador', 'Acesso limitado');
INSERT INTO Categoria (Nome) VALUES 
('Salário'), ('Alimentação'), ('Transporte'), ('Lazer'), ('Saúde'), ('Habitação'), ('Outros');

-- Inserção do Admin com Hash gerado no momento do Insert para a senha 'admin123'
INSERT INTO Cliente (nome, email, IsAtivo, IdPerfil, PasswordHash) 
VALUES (
    'Francisco Admin', 
    'admin@finance.com', 
    1, 
    1, 
    CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', '12345'), 2)
);
GO

-- ==========================================================
-- 3. VISTAS (VIEWS)
-- ==========================================================

CREATE VIEW v_ResumoContas AS
SELECT 
    C.idConta,
    C.NomeConta,
    C.Montante AS SaldoAtual,
    CASE WHEN C.IsAberta = 1 THEN 'Ativa' ELSE 'Fechada' END AS Estado,
    CON.DataInicio,
    CL.nome AS Titular
FROM Conta C
JOIN Contrato CON ON C.idContrato = CON.idContrato
JOIN Contrato_Cliente CC ON CON.idContrato = CC.idContrato
JOIN Cliente CL ON CC.idCliente = CL.idCliente
WHERE CC.IsTitular = 1;
GO

CREATE VIEW v_GastosMensaisPorCategoria AS
SELECT 
    CAT.Nome AS Categoria,
    SUM(T.ValorTransacao) AS TotalGasto,
    MONTH(T.DataTransacao) AS Mes,
    YEAR(T.DataTransacao) AS Ano
FROM [Transacao] T
JOIN Categoria CAT ON T.idCategoria = CAT.idCategoria
WHERE T.idTipo = 2 AND T.IsConcluida = 1
GROUP BY CAT.Nome, MONTH(T.DataTransacao), YEAR(T.DataTransacao);
GO

-- ==========================================================
-- 4. LÓGICA DE NEGÓCIO (PROCEDURES, TRIGGERS & FUNCTIONS)
-- ==========================================================

CREATE PROCEDURE sp_RegistarTransacao
    @idConta INT, @NomeTransacao VARCHAR(100), @Valor DECIMAL(18,2), @idCategoria INT, @idTipo INT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO [Transacao] (idConta, NomeTransacao, ValorTransacao, idCategoria, idTipo)
        VALUES (@idConta, @NomeTransacao, @Valor, @idCategoria, @idTipo);

        IF @idTipo = 1 UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @idConta;
        ELSE IF @idTipo = 2 UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @idConta;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_TransferirFundos
    @idOrigem INT, @idDestino INT, @Valor DECIMAL(18,2)
AS
BEGIN
    IF (SELECT Montante FROM Conta WHERE idConta = @idOrigem) < @Valor
        THROW 50000, 'Saldo insuficiente.', 1;

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Conta SET Montante = Montante - @Valor WHERE idConta = @idOrigem;
        UPDATE Conta SET Montante = Montante + @Valor WHERE idConta = @idDestino;
        
        INSERT INTO [Transacao] (idConta, NomeTransacao, ValorTransacao, idTipo) VALUES 
        (@idOrigem, 'Transferência Enviada', @Valor, 2),
        (@idDestino, 'Transferência Recebida', @Valor, 1);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE sp_ApagarCliente @idCliente INT
AS
BEGIN
    UPDATE Cliente SET IsExcluido = 1, IsAtivo = 0 WHERE idCliente = @idCliente;
END;
GO

-- Função de Validação de Login com Hash Interno
CREATE OR ALTER FUNCTION fn_ValidarLogin (@Email VARCHAR(100), @Password VARCHAR(255))
RETURNS INT
AS
BEGIN
    DECLARE @idCliente INT;

    SELECT @idCliente = idCliente
    FROM Cliente
    WHERE Email = @Email 
      -- Comparamos texto com texto (convertendo a senha digitada em Hexadecimal)
      AND PasswordHash = CONVERT(VARCHAR(250), HASHBYTES('SHA2_256', @Password), 2)
      AND IsAtivo = 1 
      AND IsExcluido = 0;

    RETURN ISNULL(@idCliente, 0);
END
GO

-- Trigger para registar mudanças de saldo automaticamente
CREATE TRIGGER trg_AuditoriaConta_Update
ON Conta
AFTER UPDATE
AS
BEGIN
    -- Só dispara se o campo 'Montante' tiver sido alterado
    IF UPDATE(Montante)
    BEGIN
        INSERT INTO Auditoria_Saldo (idConta, SaldoAntigo, SaldoNovo, DataAlteracao, Usuario)
        SELECT 
            i.idConta,
            d.Montante, -- d = deleted (A tabela fantasma com o valor antigo)
            i.Montante, -- i = inserted (A tabela fantasma com o valor novo)
            GETDATE(),
            'Sistema Financeiro API' -- Aqui dizemos quem fez a alteração
        FROM inserted i
        INNER JOIN deleted d ON i.idConta = d.idConta
        WHERE i.Montante <> d.Montante; -- Só regista se o valor mudou mesmo
    END
END

-- Índices de Performance (Desempenho)
CREATE NONCLUSTERED INDEX IX_Transacao_idConta ON [Transacao](idConta);
CREATE NONCLUSTERED INDEX IX_Transacao_DataTransacao ON [Transacao](DataTransacao);
CREATE NONCLUSTERED INDEX IX_Transacao_idTipo_IsConcluida ON [Transacao](idTipo, IsConcluida);

PRINT 'Base de Dados Finance configurada com sucesso!';