/*Função de Validar Login */
GO
CREATE FUNCTION fn_ValidarLogin
(
    @email VARCHAR(100),
    @passwordHash VARCHAR(255)
)
RETURNS INT
AS
BEGIN
    DECLARE @id INT;

    SELECT @id = idCliente 
    FROM Cliente 
    WHERE email = @email 
      AND by_pass = @passwordHash 
      AND IsAtivo = 1 
      AND IsExcluido = 0;

    RETURN @id;
END;
GO