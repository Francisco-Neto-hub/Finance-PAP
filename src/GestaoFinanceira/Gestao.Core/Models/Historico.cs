using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    public class Historico
    {
        public int IdHistorico { get; set; }
        public int IdMovimento { get; set; }
        public DateTime DataAlteracao { get; set; }
        public string ColunaAlterada { get; set; } = string.Empty;
        public string ValorAntigo { get; set; } = string.Empty;
        public string ValorNovo { get; set; } = string.Empty;
        public int IdUtilizador { get; set; }
    }
}
