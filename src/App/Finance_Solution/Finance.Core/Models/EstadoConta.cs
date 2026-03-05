using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class EstadoConta
    {
        public int IdEstado { get; set; }
        public string Designacao { get; set; } = string.Empty;

        // Relacionamento: Um estado pode ser aplicado a várias contas
        public virtual ICollection<Conta> Contas { get; set; } = new List<Conta>();
    }
}
