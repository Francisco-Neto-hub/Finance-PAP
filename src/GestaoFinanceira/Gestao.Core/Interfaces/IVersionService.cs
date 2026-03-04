using Gestao.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Interfaces
{
    public interface IVersionService
    {
        VersaoSoftware ObterUltimaVersao();
        bool VerificarSeVersaoEAtual(string versaoAtualApp);
    }
}
