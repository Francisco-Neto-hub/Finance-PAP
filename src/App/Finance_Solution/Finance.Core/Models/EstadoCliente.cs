using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Models
{
    public class EstadoCliente
    {
        public int IdEstado { get; set; }
        public string Designacao { get; set; } = string.Empty;

        // Relacionamento: Um estado pode estar associado a vários clientes
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
