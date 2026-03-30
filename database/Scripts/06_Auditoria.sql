CREATE TABLE Auditoria_Saldo (
    idLog INT PRIMARY KEY IDENTITY(1,1),
    idConta INT,
    SaldoAntigo DECIMAL(18,2),
    SaldoNovo DECIMAL(18,2),
    DataAlteracao DATETIME DEFAULT GETDATE(),
    Usuario VARCHAR(50) -- Pode ser o e-mail do admin que fez a alteração
);
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