using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Interfaces
{
    public interface IAuditService
    {
        void RegistarAlteracao(int idMovimento, string coluna, string antigo, string novo, int idUser);
    }
}
