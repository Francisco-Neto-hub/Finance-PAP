using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class TipoMovimento
    {
        public int IdTipo { get; set; }
        public string Descricao { get; set; } = string.Empty; // Ex: "Receita" ou "Despesa"

        // Relacionamento: Um tipo pode estar em várias transações
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
