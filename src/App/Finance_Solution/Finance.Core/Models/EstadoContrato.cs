using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class EstadoContrato
    {
        public int IdEstado { get; set; }
        public string Designacao { get; set; } = string.Empty;

        // Relacionamento: Um estado pode ser aplicado a vários contratos
        public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
