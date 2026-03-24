-- 3. POVOAMENTO INICIAL (SEED DATA)
INSERT INTO Tipo_Movimento (descricao) VALUES ('Receita'), ('Despesa');

INSERT INTO Perfil (NomePerfil, Descricao) VALUES 
('Admin', 'Acesso total ao sistema e gestão de utilizadores'),
('Utilizador', 'Acesso apenas aos próprios movimentos financeiros');

INSERT INTO Categoria (Nome) VALUES 
('Salário'), ('Alimentação'), ('Transporte'), ('Lazer'), ('Saúde'), ('Habitação');

-- 4. DADOS DE TESTE
-- Nota: Para o campo BIT, inserimos 1 ou 0.
INSERT INTO Cliente (nome, email, IsAtivo, IdPerfil, by_pass) 
VALUES ('Francisco Admin', 'admin@finance.com', 1, 1, '5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5');
