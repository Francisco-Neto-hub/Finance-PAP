using Dapper;
using Gestao.Core.Exceptions;
using Gestao.Core.Interfaces;
using Gestao.Core.Models;
using Gestao.Core.Validators;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gestao.Core.Services
{
    /// <summary>
    /// Gerencia as operações financeiras de entrada e saída, saldos e relatórios.
    /// Esta classe atua como o motor de regras de negócio para todas as transações.
    /// </summary>
    public class MovimentoService : IMovimentoService
    {
        private readonly DbConnectionFactory _dbFactory;

        /// <summary>
        /// Inicializa uma nova instância do serviço de movimentos.
        /// </summary>
        /// <param name="dbFactory">A fábrica de conexões para acesso à base de dados.</param>
        public MovimentoService(DbConnectionFactory dbFactory) => _dbFactory = dbFactory;

        /// <summary>
        /// Regista uma nova transação financeira na base de dados.
        /// </summary>
        /// <param name="movimento">Objeto contendo os dados do movimento (Data, Valor, IDs, etc).</param>
        /// <exception cref="BusinessException">Lançada se o valor for menor ou igual a zero.</exception>
        /// <exception cref="DatabaseException">Lançada em caso de erro técnico na comunicação com o SQL.</exception>
        public void RegistarMovimento(Movimento movimento)
        {
            // Chama o validador
            var validation = MovimentoValidator.Validate(movimento);

            if (!validation.IsValid)
            {
                // Junta todos os erros numa única mensagem para a BusinessException
                throw new BusinessException(string.Join(" | ", validation.Errors));
            }

            // Se passou, segue para o SQL...
            try
            {
                using var db = _dbFactory.CreateConnection();

                // Validação de Negócio: Impede que o sistema aceite valores negativos ou nulos
                if (movimento.Valor <= 0)
                    throw new BusinessException("O valor do movimento tem de ser positivo.");

                string sql = @"INSERT INTO Movimento (data, valor, descricao, id_conta, id_categoria, id_tipo_movimento, ativo) 
                               VALUES (@Data, @Valor, @Descricao, @IdConta, @IdCategoria, @IdTipoMovimento, 1)";

                db.Execute(sql, movimento);
            }
            catch (SqlException ex)
            {
                // Encapsula o erro do SQL numa exceção mais amigável para a UI
                throw new DatabaseException("Não foi possível comunicar com a Base de Dados. Verifica a tua ligação.", ex);
            }
            catch (Exception ex) when (!(ex is BusinessException))
            {
                // Captura erros inesperados que não foram previstos
                throw new Exception("Ocorreu um erro crítico no sistema.", ex);
            }
        }

        /// <summary>
        /// Obtém o saldo atual de uma conta específica através da View otimizada no SQL.
        /// </summary>
        /// <param name="idConta">ID identificador da conta bancária.</param>
        /// <returns>O valor decimal do saldo atual (Entradas - Saídas).</returns>
        public decimal ObterSaldoTotal(int idConta)
        {
            using var db = _dbFactory.CreateConnection();

            // A View vw_SaldoActual já contém a lógica de soma e subtração processada pelo SQL
            return db.ExecuteScalar<decimal>(
                "SELECT saldo_actual FROM vw_SaldoActual WHERE id_conta = @idConta",
                new { idConta });
        }

        /// <summary>
        /// Lista todos os movimentos ativos de uma conta, ordenados do mais recente para o mais antigo.
        /// </summary>
        /// <param name="idConta">ID da conta cujos movimentos serão listados.</param>
        /// <returns>Uma coleção de movimentos financeiros ativos.</returns>
        public IEnumerable<Movimento> ListarPorConta(int idConta)
        {
            using var db = _dbFactory.CreateConnection();

            return db.Query<Movimento>(
                "SELECT * FROM Movimento WHERE id_conta = @idConta AND ativo = 1 ORDER BY data DESC",
                new { idConta });
        }

        public void AtualizarMovimento(Movimento m, decimal valorAntigo, int idUserLogado)
        {
            using var db = _dbFactory.CreateConnection();

            // 1. Atualiza o movimento
            string sqlUpdate = "UPDATE Movimento SET valor = @Valor, descricao = @Descricao WHERE id_movimento = @IdMovimento";
            db.Execute(sqlUpdate, m);

            // 2. Se o valor mudou, regista no histórico
            if (m.Valor != valorAntigo)
            {
                var audit = new AuditService(_dbFactory);
                audit.RegistarAlteracao(m.IdMovimento, "valor", valorAntigo.ToString(), m.Valor.ToString(), idUserLogado);
            }
        }

        /// <summary>
        /// Realiza a eliminação lógica de um movimento (soft delete).
        /// O registo permanece na BD mas deixa de ser contabilizado nos totais.
        /// </summary>
        /// <param name="idMovimento">ID do movimento a ser desativado.</param>
        public void DesativarMovimento(int idMovimento)
        {
            using var db = _dbFactory.CreateConnection();

            db.Execute("UPDATE Movimento SET ativo = 0 WHERE id_movimento = @idMovimento",
                new { idMovimento });
        }

        /// <summary>
        /// Gera dados estatísticos de gastos agrupados por categoria.
        /// Inclui o cálculo de percentagem para facilitar a renderização de gráficos na UI.
        /// </summary>
        /// <param name="idConta">ID da conta para análise de gastos.</param>
        /// <returns>Uma lista de <see cref="RelatorioCategoria"/> com totais e percentagens por categoria.</returns>
        public IEnumerable<RelatorioCategoria> ObterGastosPorCategoria(int idConta)
        {
            using var db = _dbFactory.CreateConnection();

            // SQL que agrupa por categoria e soma apenas as Despesas (Tipo 2)
            string sql = @"
                SELECT 
                    c.nome AS CategoriaNome, 
                    SUM(m.valor) AS TotalGasto
                FROM Movimento m
                INNER JOIN Categoria c ON m.id_categoria = c.id_categoria
                WHERE m.id_conta = @idConta 
                AND m.id_tipo_movimento = 2 
                AND m.ativo = 1
                GROUP BY c.nome";

            var resultados = db.Query<RelatorioCategoria>(sql, new { idConta }).ToList();

            // Calcula o peso percentual de cada categoria no orçamento total de despesas
            decimal totalGeral = resultados.Sum(r => r.TotalGasto);
            if (totalGeral > 0)
            {
                foreach (var item in resultados)
                {
                    item.Percentagem = (double)((item.TotalGasto / totalGeral) * 100);
                }
            }

            return resultados;
        }
    }
}
