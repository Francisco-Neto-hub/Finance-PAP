using System;
using System.Collections.Generic;
using System.Text;
using Gestao.Core.Models;

namespace Gestao.Core.Interfaces
{
    public interface IUserRepository
    {
        Utilizador ObterPorEmail(string email);
        void CriarUtilizador(Utilizador utilizador);
        bool ValidarLogin(string email, string password);
    }
}
