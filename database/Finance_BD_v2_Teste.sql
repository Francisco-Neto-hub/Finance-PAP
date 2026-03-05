-- 1. Criar um Cliente
INSERT INTO Cliente (nome, email, idEstadoCliente) 
VALUES ('Francisco Neto', 'francisco@email.com', 1);

-- 2. Criar um Contrato para este cliente
INSERT INTO Contrato (DataInicio, idEstado_Contrato) 
VALUES (GETDATE(), 1);

-- 3. Ligar o Cliente ao Contrato
INSERT INTO Contrato_Cliente (idContrato, idCliente, idEstadoContratoCliente) 
VALUES (1, 1, 1);

-- 4. Criar uma Conta associada ao contrato
INSERT INTO Conta (NomeConta, idEstadoConta, Montante, DataInicio, idContrato) 
VALUES ('Conta Corrente Principal', 1, 500.00, GETDATE(), 1);

-- 5. Registar uma Transação (Ex: Compra de Almoço)
INSERT INTO [Transacao] (idConta, idEstadoTransacao, NomeTransacao, ValorTransacao, idCategoria, idTipo)
VALUES (1, 1, 'Almoço Restaurante', 15.50, 2, 2); -- 2=Alimentação, 2=Despesa