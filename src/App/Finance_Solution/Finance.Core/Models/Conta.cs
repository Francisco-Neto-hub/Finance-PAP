using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class Conta
    {
        public int IdConta { get; set; }
        public string NomeConta { get; set; } = string.Empty;
        public decimal Montante { get; set; }
        public int IdContrato { get; set; }
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
