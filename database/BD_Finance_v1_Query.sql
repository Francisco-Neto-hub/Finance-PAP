/* =========================================================
   PROJETO FINANCE - SCRIPT CONSOLIDADO (v1.0)
   Data de Criação: 02/03/2026
   Descrição: Reúne Criação, Alterações, Melhorias e Dados.
=========================================================
*/

-- 1️⃣ CRIAÇÃO DA BASE DE DADOS
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BD_Finance_v1')
BEGIN
    CREATE DATABASE BD_Finance_v1;
END
GO

USE BD_Finance_v1;
GO

-- 2️⃣ CRIAÇÃO DAS TABELAS (Ordem por Dependência)

-- Tabela: Utilizador (Já com a melhoria 'password_hash')
CREATE TABLE Utilizador (
    id_utilizador INT IDENTITY PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL -- Campo atualizado conforme melhoria de segurança
);

-- Tabela: Tipo_Movimento
CREATE TABLE Tipo_Movimento (
    id_tipo_movimento INT IDENTITY PRIMARY KEY,
    descricao VARCHAR(50) NOT NULL
);

-- Tabela: Categoria
CREATE TABLE Categoria (
    id_categoria INT IDENTITY PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    id_utilizador INT,

    CONSTRAINT fk_categoria_utilizador FOREIGN KEY (id_utilizador) REFERENCES Utilizador(id_utilizador),
    CONSTRAINT uq_categoria_utilizador UNIQUE (nome, id_utilizador) -- Impede nomes duplicados para o mesmo user
);

-- Tabela: Conta
CREATE TABLE Conta (
    id_conta INT IDENTITY PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    saldo_inicial DECIMAL(10,2) NOT NULL,
    id_utilizador INT NOT NULL,
    ativo BIT DEFAULT 1, -- Campo adicionado conforme melhoria v1

    CONSTRAINT fk_conta_utilizador FOREIGN KEY (id_utilizador) REFERENCES Utilizador(id_utilizador)
);

-- Tabela: Movimento
CREATE TABLE Movimento (
    id_movimento INT IDENTITY PRIMARY KEY,
    data DATE NOT NULL,
    valor DECIMAL(10,2) NOT NULL,
    descricao VARCHAR(255) NOT NULL, -- Alterado para NOT NULL conforme AlteraçõesBD
    id_conta INT NOT NULL,
    id_categoria INT NOT NULL,
    id_tipo_movimento INT NOT NULL,
    data_criacao DATETIME DEFAULT GETDATE(),
    ativo BIT DEFAULT 1,

    CONSTRAINT chk_valor_positivo CHECK (valor > 0),
    CONSTRAINT fk_movimento_conta FOREIGN KEY (id_conta) REFERENCES Conta(id_conta),
    CONSTRAINT fk_movimento_categoria FOREIGN KEY (id_categoria) REFERENCES Categoria(id_categoria),
    CONSTRAINT fk_movimento_tipo FOREIGN KEY (id_tipo_movimento) REFERENCES Tipo_Movimento(id_tipo_movimento)
);

-- Tabela: Alertas_Notificacoes
CREATE TABLE Alertas_Notificacoes (
    id_alerta INT IDENTITY PRIMARY KEY,
    descricao VARCHAR(255) NOT NULL
);

-- Tabela: Utilizador_Alertas
CREATE TABLE Utilizador_Alertas (
    id_utilizador INT NOT NULL,
    id_conta INT NOT NULL,
    id_alerta INT NOT NULL,
    data_criacao DATETIME DEFAULT GETDATE(),

    CONSTRAINT pk_utilizador_alertas PRIMARY KEY (id_utilizador, id_conta, id_alerta),
    CONSTRAINT fk_ua_utilizador FOREIGN KEY (id_utilizador) REFERENCES Utilizador(id_utilizador),
    CONSTRAINT fk_ua_conta FOREIGN KEY (id_conta) REFERENCES Conta(id_conta),
    CONSTRAINT fk_ua_alerta FOREIGN KEY (id_alerta) REFERENCES Alertas_Notificacoes(id_alerta)
);

-- Tabela: Historico
CREATE TABLE Historico (
    id_historico INT IDENTITY PRIMARY KEY,
    id_movimento INT NOT NULL,
    data_alteracao DATETIME DEFAULT GETDATE(),
    coluna_alterada VARCHAR(50) NOT NULL,
    valor_antigo VARCHAR(255),
    valor_novo VARCHAR(255),
    id_utilizador INT,
    
    CONSTRAINT fk_historico_movimento FOREIGN KEY (id_movimento) REFERENCES Movimento(id_movimento),
    CONSTRAINT fk_historico_utilizador FOREIGN KEY (id_utilizador) REFERENCES Utilizador(id_utilizador)
);

-- Tabela: Versões do Software (Suporte Web/Desktop)
CREATE TABLE Versoes_Software (
    id_versao INT IDENTITY PRIMARY KEY,
    versao VARCHAR(20) NOT NULL,
    data_release DATETIME DEFAULT GETDATE(),
    url_download VARCHAR(500) NOT NULL,
    notas_versao TEXT
);
GO

-- 3️⃣ LÓGICA: VIEWS E TRIGGERS

-- View para Cálculo de Saldos em Tempo Real
CREATE VIEW vw_SaldoActual AS
SELECT 
    c.id_conta,
    c.nome AS nome_conta,
    c.id_utilizador,
    c.saldo_inicial,
    ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = c.id_conta AND id_tipo_movimento = 1 AND ativo = 1), 0) AS total_entradas,
    ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = c.id_conta AND id_tipo_movimento = 2 AND ativo = 1), 0) AS total_saidas,
    (c.saldo_inicial + 
     ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = c.id_conta AND id_tipo_movimento = 1 AND ativo = 1), 0) - 
     ISNULL((SELECT SUM(valor) FROM Movimento WHERE id_conta = c.id_conta AND id_tipo_movimento = 2 AND ativo = 1), 0)
    ) AS saldo_actual
FROM Conta c;
GO

-- Trigger: Impedir Movimentos Duplicados Exactos
CREATE TRIGGER trg_Prevent_Duplicate_Movimento
ON Movimento
INSTEAD OF INSERT
AS
BEGIN
    INSERT INTO Movimento (data, valor, descricao, id_conta, id_categoria, id_tipo_movimento, ativo, data_criacao)
    SELECT i.data, i.valor, i.descricao, i.id_conta, i.id_categoria, i.id_tipo_movimento, 
           COALESCE(i.ativo, 1), COALESCE(i.data_criacao, GETDATE())
    FROM inserted i
    WHERE NOT EXISTS (
        SELECT 1 FROM Movimento m
        WHERE m.data = i.data
          AND m.valor = i.valor
          AND ISNULL(m.descricao,'') = ISNULL(i.descricao,'')
          AND m.id_conta = i.id_conta
          AND m.id_categoria = i.id_categoria
          AND m.id_tipo_movimento = i.id_tipo_movimento
          AND m.ativo = 1
    );
END;
GO

-- Trigger: Registar Histórico de Alterações em Movimentos
CREATE TRIGGER trg_Historico_Movimento
ON Movimento
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id_utilizador INT = NULL;

    INSERT INTO Historico (id_movimento, coluna_alterada, valor_antigo, valor_novo, id_utilizador)
    SELECT i.id_movimento, 'valor', CAST(d.valor AS VARCHAR), CAST(i.valor AS VARCHAR), @id_utilizador
    FROM inserted i JOIN deleted d ON i.id_movimento = d.id_movimento WHERE i.valor <> d.valor;

    INSERT INTO Historico (id_movimento, coluna_alterada, valor_antigo, valor_novo, id_utilizador)
    SELECT i.id_movimento, 'descricao', d.descricao, i.descricao, @id_utilizador
    FROM inserted i JOIN deleted d ON i.id_movimento = d.id_movimento WHERE i.descricao <> d.descricao;

    INSERT INTO Historico (id_movimento, coluna_alterada, valor_antigo, valor_novo, id_utilizador)
    SELECT i.id_movimento, 'ativo', CAST(d.ativo AS VARCHAR), CAST(i.ativo AS VARCHAR), @id_utilizador
    FROM inserted i JOIN deleted d ON i.id_movimento = d.id_movimento WHERE i.ativo <> d.ativo;

    INSERT INTO Historico (id_movimento, coluna_alterada, valor_antigo, valor_novo, id_utilizador)
    SELECT i.id_movimento, 'id_categoria', CAST(d.id_categoria AS VARCHAR), CAST(i.id_categoria AS VARCHAR), @id_utilizador
    FROM inserted i JOIN deleted d ON i.id_movimento = d.id_movimento WHERE i.id_categoria <> d.id_categoria;
END;
GO

-- 4️⃣ INSERÇÃO DE DADOS (Seed Data)

INSERT INTO Utilizador (nome, email, password_hash) VALUES
('João Silva', 'joao@teste.pt', 'hash123'),
('Maria Costa', 'maria@teste.pt', 'hash456');

INSERT INTO Conta (nome, saldo_inicial, id_utilizador) VALUES
('Conta à Ordem', 1500.00, 1),
('Poupança', 3000.00, 1),
('Conta Principal', 2200.00, 2);

INSERT INTO Tipo_Movimento (descricao) VALUES ('Entrada'), ('Saída');

INSERT INTO Categoria (nome, id_utilizador) VALUES
('Salário', 1), ('Alimentação', 1), ('Renda', 2);

-- Movimentos iniciais
INSERT INTO Movimento (data, valor, descricao, id_conta, id_categoria, id_tipo_movimento) VALUES
('2026-02-01', 2500.00, 'Salário Janeiro', 1, 1, 1),
('2026-02-03', 120.50, 'Supermercado', 1, 2, 2),
('2026-02-05', 700.00, 'Pagamento Renda', 3, 3, 2);

INSERT INTO Alertas_Notificacoes (descricao) VALUES
('Saldo baixo'), ('Movimento elevado'), ('Novo movimento registado');

INSERT INTO Utilizador_Alertas (id_utilizador, id_conta, id_alerta) VALUES
(1, 1, 1), (1, 1, 2), (1, 2, 1), (2, 3, 3);

INSERT INTO Versoes_Software (versao, url_download, notas_versao)
VALUES ('1.0.0', 'https://github.com/PAP-Finance/releases/download/v1.0/finance.exe', 'Versão inicial de lançamento.');

GO

-- 5️⃣ CONSULTA DE VERIFICAÇÃO (Opcional)
SELECT * FROM vw_SaldoActual;