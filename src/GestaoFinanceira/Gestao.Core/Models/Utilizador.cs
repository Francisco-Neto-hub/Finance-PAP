using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    public class Utilizador
    {
        public int IdUtilizador { get; set; } // id_utilizador
        public string Nome { get; set; } = string.Empty; // nome
        public string Email { get; set; } = string.Empty; // email
        public string PasswordHash { get; set; } = string.Empty; // password_hash
    }
}
