using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class EstadoContratoCliente
    {
        public int IdEstado { get; set; }
        public string Designacao { get; set; } = string.Empty;

        // Relacionamento: Este estado pode ser aplicado a várias ligações contrato-cliente
        public virtual ICollection<ContratoCliente> ContratosClientes { get; set; } = new List<ContratoCliente>();
    }
}
