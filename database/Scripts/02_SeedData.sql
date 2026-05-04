-- 3. POVOAMENTO INICIAL (SEED DATA)
INSERT INTO Tipo_Movimento (descricao) VALUES ('Receita'), ('Despesa'), ('Transferência');

INSERT INTO Perfil (NomePerfil, Descricao) VALUES 
('Admin', 'Acesso total ao sistema e gestão de utilizadores'),
('Utilizador', 'Acesso apenas aos próprios movimentos financeiros');

INSERT INTO Categoria (Nome) VALUES 
('Salário'), ('Alimentação'), ('Transporte'), ('Lazer'), ('Saúde'), ('Habitação'), ('Outros');

-- 4. DADOS DE TESTE
-- Inserção do Admin com Hash gerado no momento do Insert para a senha 'admin123'
INSERT INTO Cliente (nome, email, IsAtivo, IdPerfil, PasswordHash) 
VALUES (
    'Francisco Admin', 
    'admin@finance.com', 
    1, 
    1, 
    CONVERT(VARCHAR(255), HASHBYTES('SHA2_256', '12345'), 2)
);
GO
