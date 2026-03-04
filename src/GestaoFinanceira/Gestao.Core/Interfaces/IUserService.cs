using System;
using System.Collections.Generic;
using System.Text;
using Gestao.Core.Models;

namespace Gestao.Core.Interfaces
{
    /// <summary>
    /// Interface que define os serviços de gestão de utilizadores.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Regista um novo utilizador no sistema com validação e segurança.
        /// </summary>
        /// <param name="utilizador">Objeto com dados do utilizador.</param>
        /// <param name="passwordAberta">Password em texto limpo para ser encriptada.</param>
        void RegistarUtilizador(Utilizador utilizador, string passwordAberta);
    }
}
