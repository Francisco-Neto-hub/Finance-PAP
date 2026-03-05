using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class EstadoTransacao
    {
        public int IdEstado { get; set; }
        public string Designacao { get; set; } = string.Empty;

        // Relacionamento: Um estado pode ser aplicado a várias transações
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
