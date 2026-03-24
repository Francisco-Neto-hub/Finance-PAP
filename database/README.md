# Base de Dados - Finance System

Este diretório contém os scripts SQL Server para o sistema de gestão financeira.

## Ordem de Execução:
1. `01_Estrutura.sql`: Cria as tabelas base com suporte a Soft Delete e campos Booleanos (BIT).
2. `02_SeedData.sql`: Povoa as tabelas de sistema (Perfis e Categorias).
3. `03_Views.sql`: Cria as vistas para o Dashboard.
4. `04_Procedures.sql`: Implementa a lógica de negócio (Transações e Transferências).
5. `05_Functions.sql`: Implementa a lógica de autenticação.

**Nota:** Todos os estados são geridos via campos `BIT` para otimização de memória e performance.