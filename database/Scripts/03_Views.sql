/* Views */

/* View para o Dashboard */
CREATE VIEW v_ResumoContas AS
SELECT 
    C.NomeConta,
    C.Montante AS SaldoAtual,
    CASE WHEN C.IsAberta = 1 THEN 'Ativa' ELSE 'Fechada' END AS Estado,
    CON.DataInicio, -- Corrigido de CT para CON
    CL.nome AS Titular
FROM Conta C
JOIN Contrato CON ON C.idContrato = CON.idContrato
JOIN Contrato_Cliente CC ON CON.idContrato = CC.idContrato
JOIN Cliente CL ON CC.idCliente = CL.idCliente
WHERE CC.IsTitular = 1;

/* Relatório Mensal: Despesas por Categoria */
CREATE VIEW v_GastosMensaisPorCategoria AS
SELECT 
    CAT.Nome AS Categoria,
    SUM(T.ValorTransacao) AS TotalGasto,
    MONTH(T.DataTransacao) AS Mes,
    YEAR(T.DataTransacao) AS Ano
FROM [Transacao] T
JOIN Categoria CAT ON T.idCategoria = CAT.idCategoria
WHERE T.idTipo = 2 -- Apenas 'Despesa'
  AND T.IsConcluida = 1
GROUP BY CAT.Nome, MONTH(T.DataTransacao), YEAR(T.DataTransacao);