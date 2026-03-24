/*Query de Auditoria */
SELECT 
    SUM(CASE WHEN idTipo = 1 THEN ValorTransacao ELSE -ValorTransacao END) AS FluxoCaixaGlobal
FROM [Transacao]
WHERE IsConcluida = 1;