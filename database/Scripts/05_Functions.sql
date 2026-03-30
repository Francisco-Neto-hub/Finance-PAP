/* Função de Validar Login com Hashing SHA-256 */
GO

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