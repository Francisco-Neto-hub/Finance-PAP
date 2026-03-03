using System;
using System.Collections.Generic;
using System.Text;
using Gestao.Core.Models;

namespace Gestao.Core.Interfaces
{
    public interface IMovimentoService
    {
        void RegistarMovimento(Movimento movimento);
        IEnumerable<Movimento> ListarPorConta(int idConta);
        void DesativarMovimento(int idMovimento); // Soft delete conforme o teu SQL (ativo = 0)
    }
}
