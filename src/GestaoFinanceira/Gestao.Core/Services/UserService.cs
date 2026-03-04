using Dapper;
using Gestao.Core.Exceptions;
using Gestao.Core.Helpers;
using Gestao.Core.Interfaces;
using Gestao.Core.Models;
using Gestao.Core.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de utilizadores, incluindo o registo de novas contas.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DbConnectionFactory _dbFactory;

        public UserService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Realiza o registo de um novo utilizador com validação de dados e hashing de password.
        /// </summary>
        /// <param name="utilizador">Objeto com os dados do utilizador.</param>
        /// <param name="passwordAberta">A password em texto limpo vinda da UI.</param>
        public void RegistarUtilizador(Utilizador utilizador, string passwordAberta)
        {
            // 1. Validação de dados (Nome, Email e Força da Password)
            var validation = UserValidator.Validate(utilizador, passwordAberta);
            if (!validation.IsValid)
            {
                throw new BusinessException(string.Join(" | ", validation.Errors));
            }

            using var db = _dbFactory.CreateConnection();

            // 2. Verificar se o e-mail já existe
            var existe = db.ExecuteScalar<int>("SELECT COUNT(1) FROM Utilizador WHERE email = @Email", new { utilizador.Email });
            if (existe > 0)
            {
                throw new BusinessException("Este endereço de e-mail já está registado.");
            }

            // 3. Gerar o Hash Seguro da Password
            utilizador.PasswordHash = PasswordHasher.HashPassword(passwordAberta);

            // 4. Inserir na Base de Dados
            string sql = @"INSERT INTO Utilizador (nome, email, password_hash, ativo) 
                           VALUES (@Nome, @Email, @PasswordHash, 1)";

            try
            {
                db.Execute(sql, utilizador);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Erro ao criar a conta de utilizador.", ex);
            }
        }
    }
}
