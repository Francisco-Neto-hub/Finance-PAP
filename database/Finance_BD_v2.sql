/* PROJETO: Finance - Sistema de Gestão de Contas (Versão SQL Server)
   AUTORES: Francisco Neto e Francisco Loureiro
   DESCRIÇÃO: Script unificado com Estrutura, Perfis (Admin/User) e Seed Data.
*/

-- 1. CRIAR AS TABELAS DE ESTADO (Lookup Tables)
CREATE TABLE Estado_Cliente (
    idEstado INT PRIMARY KEY IDENTITY(1,1),
    designacao VARCHAR(50) NOT NULL
);

CREATE TABLE Estado_Contrato (
    idEstado INT PRIMARY KEY IDENTITY(1,1),
    designacao VARCHAR(50) NOT NULL
);

CREATE TABLE Estado_Contrato_Cliente (
    idEstado INT PRIMARY KEY IDENTITY(1,1),
    designacao VARCHAR(50) NOT NULL
);

CREATE TABLE Estado_Conta (
    idEstado INT PRIMARY KEY IDENTITY(1,1),
    designacao VARCHAR(50) NOT NULL
);

CREATE TABLE Estado_Transacao (
    idEstado INT PRIMARY KEY IDENTITY(1,1),
    designacao VARCHAR(50) NOT NULL
);

-- 2. CRIAR TABELAS DE CATEGORIZAÇÃO E PERFIS
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
    NomePerfil VARCHAR(50) NOT NULL, -- 'Admin', 'Utilizador'
    Descricao VARCHAR(255)
);

-- 3. CRIAR ENTIDADES PRINCIPAIS
CREATE TABLE Cliente (
    idCliente INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(100) NOT NULL,
    telemovel VARCHAR(20),
    email VARCHAR(100) UNIQUE NOT NULL,
    DataNasc DATE,
    idEstadoCliente INT,
    IdPerfil INT NOT NULL DEFAULT 2, -- Por padrão: Utilizador
    by_pass VARCHAR(255) NOT NULL DEFAULT '12345',
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (idEstadoCliente) REFERENCES Estado_Cliente(idEstado),
    FOREIGN KEY (IdPerfil) REFERENCES Perfil(IdPerfil)
);

CREATE TABLE Contrato (
    idContrato INT PRIMARY KEY IDENTITY(1,1),
    DataInicio DATE NOT NULL,
    DataFim DATE,
    idEstado_Contrato INT,
    FOREIGN KEY (idEstado_Contrato) REFERENCES Estado_Contrato(idEstado)
);

CREATE TABLE Contrato_Cliente (
    idContrato INT,
    idCliente INT,
    idEstadoContratoCliente INT,
    PRIMARY KEY (idContrato, idCliente),
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato),
    FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente),
    FOREIGN KEY (idEstadoContratoCliente) REFERENCES Estado_Contrato_Cliente(idEstado)
);

CREATE TABLE Conta (
    idConta INT PRIMARY KEY IDENTITY(1,1),
    NomeConta VARCHAR(100) NOT NULL,
    idEstadoConta INT,
    Montante DECIMAL(18,2) DEFAULT 0.00,
    DataInicio DATE NOT NULL,
    DataFim DATE,
    idContrato INT,
    FOREIGN KEY (idEstadoConta) REFERENCES Estado_Conta(idEstado),
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato)
);

CREATE TABLE [Transacao] (
    idTransacao INT PRIMARY KEY IDENTITY(1,1),
    idConta INT NOT NULL,
    idEstadoTransacao INT,
    NomeTransacao VARCHAR(100) NOT NULL,
    DataTransacao DATETIME NOT NULL DEFAULT GETDATE(),
    ValorTransacao DECIMAL(18,2) NOT NULL,
    idCategoria INT,
    idTipo INT,     
    FOREIGN KEY (idConta) REFERENCES Conta(idConta),
    FOREIGN KEY (idEstadoTransacao) REFERENCES Estado_Transacao(idEstado),
    FOREIGN KEY (idCategoria) REFERENCES Categoria(idCategoria),
    FOREIGN KEY (idTipo) REFERENCES Tipo_Movimento(idTipo)
);

-- 4. POVOAMENTO INICIAL (SEED DATA)
INSERT INTO Estado_Cliente (designacao) VALUES ('Ativo'), ('Inativo');
INSERT INTO Estado_Contrato (designacao) VALUES ('Vigente'), ('Finalizado');
INSERT INTO Estado_Contrato_Cliente (designacao) VALUES ('Titular'), ('Beneficiário');
INSERT INTO Estado_Conta (designacao) VALUES ('Aberta'), ('Fechada');
INSERT INTO Estado_Transacao (designacao) VALUES ('Concluída'), ('Pendente');

INSERT INTO Tipo_Movimento (descricao) VALUES ('Receita'), ('Despesa');

INSERT INTO Perfil (NomePerfil, Descricao) VALUES 
('Admin', 'Acesso total ao sistema e gestão de utilizadores'),
('Utilizador', 'Acesso apenas aos próprios movimentos financeiros');

INSERT INTO Categoria (Nome) VALUES 
('Salário'), ('Alimentação'), ('Transporte'), ('Lazer'), ('Saúde'), ('Habitação');

-- 5. DADOS DE TESTE (Utilizador Admin Inicial)
INSERT INTO Cliente (nome, email, idEstadoCliente, IdPerfil, by_pass) 
VALUES ('Francisco Admin', 'admin@finance.com', 1, 1, '5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5');

PRINT 'Base de Dados Finance Unificada criada com sucesso!';