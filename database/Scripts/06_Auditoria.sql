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
CREATE TRIGGER trg_AuditarSaldo
ON Conta
AFTER UPDATE
AS
BEGIN
    IF UPDATE(Montante)
    BEGIN
        INSERT INTO Auditoria_Saldo (idConta, SaldoAntigo, SaldoNovo)
        SELECT d.idConta, d.Montante, i.Montante
        FROM deleted d
        JOIN inserted i ON d.idConta = i.idConta;
    END
END;
GO