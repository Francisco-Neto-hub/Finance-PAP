using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Relacionamento: Uma categoria pode estar em várias transações
        public virtual ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
