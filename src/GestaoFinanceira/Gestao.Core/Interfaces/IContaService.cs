using System;
using System.Collections.Generic;
using System.Text;
using Gestao.Core.Models;

namespace Gestao.Core.Interfaces
{
    public interface IContaService
    {
        IEnumerable<Conta> ListarContasPorUtilizador(int idUtilizador);
        void CriarConta(Conta conta);
        decimal ObterSaldoAtual(int idConta);
    }
}
