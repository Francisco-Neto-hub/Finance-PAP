/* PROJETO: Finance - Sistema de Gestão de Contas (Versão Otimizada)
   DESCRIÇÃO: Script com estados convertidos para campos BIT (Booleano).
*/

-- 1. CRIAR TABELAS DE CATEGORIZAÇÃO E PERFIS (Mantidas pois têm mais de 2 estados)
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

-- 2. CRIAR ENTIDADES PRINCIPAIS COM CAMPOS BIT
CREATE TABLE Cliente (
    idCliente INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(100) NOT NULL,
    telemovel VARCHAR(20),
    email VARCHAR(100) UNIQUE NOT NULL,
    DataNasc DATE,
    IsAtivo BIT DEFAULT 1, -- 1: Ativo, 0: Inativo
    IdPerfil INT NOT NULL DEFAULT 2,
    PasswordHash VARCHAR(255) NOT NULL DEFAULT '12345',
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IdPerfil) REFERENCES Perfil(IdPerfil)
);

CREATE TABLE Contrato (
    idContrato INT PRIMARY KEY IDENTITY(1,1),
    DataInicio DATE NOT NULL,
    DataFim DATE,
    IsVigente BIT DEFAULT 1, -- 1: Vigente, 0: Finalizado
);

CREATE TABLE Contrato_Cliente (
    idContrato INT,
    idCliente INT,
    IsTitular BIT DEFAULT 1, -- 1: Titular, 0: Beneficiário
    PRIMARY KEY (idContrato, idCliente),
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato),
    FOREIGN KEY (idCliente) REFERENCES Cliente(idCliente)
);

CREATE TABLE Conta (
    idConta INT PRIMARY KEY IDENTITY(1,1),
    NomeConta VARCHAR(100) NOT NULL,
    IsAberta BIT DEFAULT 1, -- 1: Aberta, 0: Fechada
    Montante DECIMAL(18,2) DEFAULT 0.00,
    DataInicio DATE NOT NULL,
    DataFim DATE,
    idContrato INT,
    FOREIGN KEY (idContrato) REFERENCES Contrato(idContrato)
);

CREATE TABLE [Transacao] (
    idTransacao INT PRIMARY KEY IDENTITY(1,1),
    idConta INT NOT NULL,
    IsConcluida BIT DEFAULT 1, -- 1: Concluída, 0: Pendente
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

CREATE TABLE Suporte_Ticket (
    idTicket INT PRIMARY KEY IDENTITY(1,1),
    idCliente INT NOT NULL,
    Assunto NVARCHAR(150) NOT NULL,
    Mensagem NVARCHAR(MAX) NOT NULL,
    DataCriacao DATETIME DEFAULT GETDATE(),
    IsResolvido BIT DEFAULT 0,
    
    -- Chave estrangeira para garantir que o ticket pertence a um cliente real
    CONSTRAINT FK_Ticket_Cliente FOREIGN KEY (idCliente) 
    REFERENCES Cliente(idCliente)
);

-- Índice para acelerar a listagem de tickets por cliente e por estado
CREATE INDEX IX_Suporte_Status ON Suporte_Ticket (IsResolvido, DataCriacao);

PRINT 'Base de Dados Finance Otimizada (BIT fields) criada com sucesso!';