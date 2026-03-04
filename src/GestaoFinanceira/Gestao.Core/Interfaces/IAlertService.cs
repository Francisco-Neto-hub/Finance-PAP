using System;
using System.Collections.Generic;
using Gestao.Core.Models;
using System.Text;

namespace Gestao.Core.Interfaces
{
    public interface IAlertService
    {
        // Obtém alertas como "Saldo Baixo" ou "Nova Versão" para o utilizador
        IEnumerable<string> ObterAlertasAtivos(int idUtilizador);
        void CriarAlerta(int idUtilizador, int idConta, string descricao);
    }
}
